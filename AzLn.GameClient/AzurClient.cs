using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Contract.Extensions;
using AzLn.GameClient.CommandsLogger;
using AzLn.GameClient.Options;
using AzLn.Protocol;
using NLog;

namespace AzLn.GameClient
{
    /// <summary>
    /// Azur lane client
    /// </summary>
    public class AzurClient : IAzurClient
    {
        /// <summary>
        /// Invoke at first received bytes
        /// </summary>
        public event EventHandler<ClientModeChangedEvent>? OnModeChanged;
        /// <summary>
        /// Invoke when read loop break and stream closed
        /// </summary>
        public event EventHandler<EventArgs>? OnDisconnect;

        private readonly ILogger _logger;
        private readonly ICommandsLogger? _commandsLogger;
        private readonly IConnectionOptions _connOpts;

        private readonly CancellationTokenSource _taskCts = new CancellationTokenSource();
        private Task? _sendBytesLoopTask;
        private Task? _receiveLoopTask;


        private readonly BlockingCollection<AzurClientData> _receiveQueue =
            new BlockingCollection<AzurClientData>(new ConcurrentQueue<AzurClientData>());
        private readonly BlockingCollection<AzurClientData> _sendQueue =
            new BlockingCollection<AzurClientData>(new ConcurrentQueue<AzurClientData>());

        private Stream? _dataStream;
        private EClientMode _clientMode = EClientMode.Raw;

        public AzurClient(IConnectionOptions connectionOptions, ILogger logger, ICommandsLogger? commandsLogger = null)
        {
            _commandsLogger = commandsLogger;
            _connOpts = connectionOptions;
            _logger = logger;
            _logger.Trace("Instance created");
        }

        //public async Task StartAsync(string host, int port, CancellationToken cancellationToken = default)
        //{
        //    _logger.Trace("Call start with host: {Host} and port: {Port}", host, port);
        //    await Task.Factory.StartNew(() =>
        //    {
        //        _dataStream = new TcpClient(host, port).GetStream();
        //    }, cancellationToken);
        //    _logger.Trace("Successful connected");
        //    _logger.Trace("Launching read and send loops");
        //    _sendBytesLoopTask = Task.Factory.StartNew(SendLoop, cancellationToken);
        //    _receiveLoopTask = Task.Factory.StartNew(ReadLoop, cancellationToken);
        //    _logger.Trace("Client started");
        //}
        public void Start(Stream stream)//, CancellationToken cancellationToken = default)
        {
            var ct = _taskCts.Token;
            _logger.Trace("Call start with stream");
            _dataStream = stream;
            _logger.Trace("Launching read and send loops");
            _sendBytesLoopTask = SendLoopAsync(ct);
            _receiveLoopTask = ReadLoopAsync(ct);
            _logger.Trace("Client started");
        }
        public async Task StartAsync(Stream stream)//, CancellationToken cancellationToken = default)
        {
            await Task.Factory.StartNew(() => Start(stream));
        }

        public async Task StopAsync()
        {
            _logger.Trace("Call StopAsync");
            _taskCts.Cancel();
            _sendQueue.CompleteAdding();
            _receiveQueue.CompleteAdding();
            _dataStream!.Close();
            _logger.Trace("_dataStream closed");
            await Task.WhenAll(_sendBytesLoopTask!, _receiveLoopTask!);
            _logger.Trace("Tasks waited");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<AzurClientData> ReadAsync(CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => _receiveQueue.Take(cancellationToken), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public AzurClientData Read(CancellationToken cancellationToken = default)
        {
            return _receiveQueue.Take(cancellationToken);
        }

        public void Write(string str, CancellationToken cancellationToken = default)
            => Write(new AzurClientData(AzurClientData.EDataType.String, str), cancellationToken);

        public void Write(ICommand command, CancellationToken cancellationToken = default) 
            => Write(new AzurClientData(AzurClientData.EDataType.Command, command), cancellationToken);

        public void Write(byte[] bytes, CancellationToken cancellationToken = default) 
            => Write(new AzurClientData(AzurClientData.EDataType.Bytes, bytes), cancellationToken);

        public void Write(AzurClientData data, CancellationToken cancellationToken = default)
        {
            _sendQueue.Add(data, cancellationToken);
        }

        public void WriteAll(IEnumerable<ICommand> commands, CancellationToken cancellationToken = default)
        {
            WriteAll(commands.Select(x=>new AzurClientData(AzurClientData.EDataType.Bytes, x.FullPSerialize())), cancellationToken);
        }

        public void WriteAll(IEnumerable<AzurClientData> data, CancellationToken cancellationToken = default)
        {
            data.ForEach(x=> _sendQueue.Add(x, cancellationToken));
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && !_sendQueue.IsCompleted)
                {
                    var data = await Task.Run(() => _sendQueue.Take(cancellationToken), cancellationToken);
                    var bytes = data.Type switch
                    {
                        AzurClientData.EDataType.String => Encoding.UTF8.GetBytes((string) data.Data),
                        AzurClientData.EDataType.Command => ((ICommand) data.Data).FullPSerialize(),
                        AzurClientData.EDataType.UnparsedCommand => ((ICommand) data.Data).FullPSerialize(),
                        AzurClientData.EDataType.Bytes => (byte[]) data.Data,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    if (_dataStream == null)
                        throw new ArgumentNullException();

                    await _dataStream.WriteAsync(bytes, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Trace("SendLoop cancelled");
            }
            _logger.Trace("Send loop exit");
        }

        private bool DetectMode(Span<byte> buffer)
        {
            var modeDetected = true;
            //0x47,0x45 == GE || 0x48,0x54 == HT
            var modeHeader = buffer[0] << 8 | buffer[1];
            if (buffer.Length >= 2 && modeHeader == 0x4745 || modeHeader == 0x4854)
            {
                _logger.Trace("Enter to http mode because receive GE or HT bytes");
                _clientMode = EClientMode.Http;
            }
            else if (buffer.Length >= 5)
            {
                modeHeader = buffer[3] << 8 | buffer[4];
                if (modeHeader >= 10_001 && modeHeader <= 64_004)
                {
                    _logger.Trace("Enter to proto mode bcs receive valid command id");
                    _clientMode = EClientMode.ProtoBuf;
                }
                else
                {
                    _logger.Warn("Fall to raw mode, header: {Header}", modeHeader);
                    _clientMode = EClientMode.Raw;
                }
            }
            else
            {
                modeDetected = false;
            }

            if (modeDetected)
            {
                OnModeChanged?.Invoke(this, new ClientModeChangedEvent(_clientMode));
            }

            return modeDetected;
        }

        private async Task ReadLoopAsync(CancellationToken cancellationToken)
        {
            if (_dataStream == null)
                throw new ArgumentNullException();

            var buffer = new byte[_connOpts.InitBufferLen];
            var offset = 0;
            var inModeDetecting = true;

            Decoder? decoder = null;
            char[]? charsBuffer = null;
            try
            {
                int read;
                while ((read = await _dataStream.ReadAsync(buffer, offset, _connOpts.ReadBatchLen, cancellationToken)) != 0)
                {
                    _logger.Trace("Receive {BytesCount} bytes", read);
                    offset += read;
                    if (inModeDetecting && DetectMode(new Span<byte>(buffer, 0, offset)))
                        inModeDetecting = false;
                    else
                        continue;

                    if (_clientMode == EClientMode.Http)
                    {
                        _logger.Trace("Receive {BytesCount} bytes in http mode", offset);
                        decoder ??= Encoding.UTF8.GetDecoder();
                        charsBuffer ??= new char[_connOpts.ReadBatchLen];

                        var charsCount = decoder.GetChars(buffer, 0, offset, charsBuffer, 0);
                        if (charsCount > 0)
                        {
                            _receiveQueue.Add(new AzurClientData()
                            {
                                Data = new string(charsBuffer, 0, charsCount),
                                Type = AzurClientData.EDataType.String
                            }, cancellationToken);
                        }
                        offset = 0;
                    }
                    else if (_clientMode == EClientMode.ProtoBuf)
                    {
                        if (offset < BaseCommand.AdvHeaderLen && _clientMode == EClientMode.ProtoBuf)
                            continue;
                        var (finBuffer, finOffset) = ReadAllCommandsFromBuffer(buffer, 0, offset);
                        offset = finOffset;
                        buffer = finBuffer;
                    }
                    else if (_clientMode == EClientMode.Raw)
                    {
                        if (offset > 0)
                        {
                            _receiveQueue.Add(new AzurClientData()
                            {
                                Data = buffer.Take(offset).ToArray(),
                                Type = AzurClientData.EDataType.Bytes
                            }, cancellationToken);
                        }
                        offset = 0;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    if (offset + _connOpts.ReadBatchLen >= buffer.Length)
                    {
                        _logger.Warn(
                            "Receive buffer smallest than need. Resizing from {CurrentBufferLen} to {FinBufferLen}",
                            buffer.Length, buffer.Length + _connOpts.BufferResizeStep); 
                        Array.Resize(ref buffer, buffer.Length + _connOpts.BufferResizeStep);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Read loop cancelled");
            }
            catch (IOException e) //when ((e.InnerException as SocketException)?.ErrorCode == 10004)
            {
                _logger.Debug("Catch io cancellation exception");
            }
            catch (Exception exception)
            {
                _logger.Warn(exception);
            }


            //disconnect event invoke
            if (_dataStream.CanRead)
                _dataStream.Close();

            _logger.Trace("Stream closed");
            OnDisconnect?.Invoke(this, EventArgs.Empty);
            //_logger.Trace("Call stopAsync from ReadLoop");
            //await StopAsync();
            //_logger.Trace("Call stopAsync from ReadLoop success");
        }

        private (byte[] buffer, int offset) ReadAllCommandsFromBuffer(byte[] buffer, int bufferStart, int endOffset)
        {
            while (true)
            {
                var (finalBufferStart, command) = ReadOneCommand(buffer, bufferStart, endOffset);

                bufferStart = finalBufferStart;
                var availableData = endOffset - bufferStart;

                if (command == null)
                {
                    Array.Copy(buffer, bufferStart, buffer, 0, availableData);
                    return (buffer, availableData);
                }

                _receiveQueue.Add(new AzurClientData()
                {
                    Data = command,
                    Type = command is UnparsedCommand
                        ? AzurClientData.EDataType.UnparsedCommand
                        : AzurClientData.EDataType.Command
                }, CancellationToken.None);
            }
        }

        private (int finBufferStart, ICommand? command) ReadOneCommand(byte[] buffer, int bufferStart, int bufferEnd)
        {
            var availableData = bufferEnd - bufferStart;

            if (availableData > 2)
            {
                var packetLength = (ushort) (buffer[bufferStart] << 8 | buffer[bufferStart + 1]);
                if (availableData >= packetLength + BaseCommand.HeaderLen)
                {
                    var commandId = (ushort) (buffer[bufferStart + 3] << 8 | buffer[bufferStart + 4]);
                    var idx = (ushort) (buffer[bufferStart + 5] << 8 | buffer[bufferStart + 6]);
                    var commandRawPayload = new byte[packetLength - (BaseCommand.AdvHeaderLen - BaseCommand.HeaderLen)];

                    _logger.Trace("Receive command: {CommandId}, idx: {Idx}, payload len: {PayloadLen}",
                        commandId, idx, commandRawPayload.Length);
                    Array.Copy(buffer, BaseCommand.AdvHeaderLen + bufferStart, commandRawPayload, 0,
                        packetLength - BaseCommand.HeaderNoIdLen);
                    _ = _commandsLogger?.LogReceiveCommandsAsync(commandRawPayload, commandId, idx);
                    ICommand? command = null;
                    if (!BaseCommand.TryPDeserialize(commandId, commandRawPayload, idx, ref command, true))
                    {
                        _logger.Warn("Can't deserialize command with id: {CommandId}", commandId);
                    }
                    return (bufferStart + packetLength + BaseCommand.HeaderLen, command);
                }
            }

            return (bufferStart, null);
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _sendBytesLoopTask?.Dispose();
            _receiveLoopTask?.Dispose();
            _sendQueue.Dispose();
            _receiveQueue.Dispose();
            _dataStream?.Dispose();
            _receiveQueue.Dispose();
            _sendQueue.Dispose();
            _taskCts?.Dispose();
        }
    }
}