using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using AzLn.Contract;
using AzLn.GameClient.Options;
using AzLn.Protocol;
using NLog;

namespace AzLn.GameClient.CommandsLogger
{
    public class CommandsLogger : ICommandsLogger
    {
        private readonly ICommandsLoggerOptions _options;
        private readonly ConcurrentBag<DbCommandLogEntry> _preWriteBuffer = new ConcurrentBag<DbCommandLogEntry>();
        private readonly ILogger _logger;
        private readonly object _locker = new object();

        public CommandsLogger(ICommandsLoggerOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _logger.Info("Binary logger created");
        }

        private void LogCommand(ECommandType commandType, byte[] commandBytes, ushort commandId, ushort idx)
        {
            var server = (EGameServer) (MappedDiagnosticsLogicalContext.GetObject("Server") ?? EGameServer.NotSet);
            var userId = (uint) (MappedDiagnosticsLogicalContext.GetObject("UserId") ?? 0);
            var guid = Guid.NewGuid();

            _preWriteBuffer.Add(new DbCommandLogEntry()
            {
                Bytes = commandBytes,
                Guid = guid,
                CommandType = commandType,
                GameServer = server,
                UserId = userId,
                CommandId = commandId,
                Index = idx
            });
            if (_options.Validate)
            {
                _preWriteBuffer.Add(new DbCommandLogEntry()
                {
                    Bytes = BaseCommand.PDeserialize(commandId, commandBytes, idx).PayloadPSerialize(),
                    Guid = guid,
                    CommandType = commandType,
                    GameServer = server,
                    UserId = userId,
                    CommandId = commandId,
                    Index = idx
                });
            }

            if (_preWriteBuffer.Count < _options.PreWriteBufferLen) 
                return;

            lock (_locker)
            {
                if (_options.Enable)
                {
                    _options.DbContext!.Commands.AddRange(_preWriteBuffer.ToArray());
                    _options.DbContext!.SaveChanges();
                }
            }
        }

        public async Task LogReceiveCommandsAsync(byte[] commandBytes, ushort commandId, ushort idx)
        {
            await Task.Factory.StartNew(() => LogCommand(ECommandType.Receive, commandBytes, commandId, idx));
        }

        public async Task LogSendCommandAsync(byte[] commandBytes, ushort commandId, ushort idx)
        {
            await Task.Factory.StartNew(() => LogCommand(ECommandType.Send, commandBytes, commandId, idx));
        }
    }
}