using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzLn.Protocol;

namespace AzLn.GameClient
{
    public interface IAzurClient: IDisposable
    {
        event EventHandler<ClientModeChangedEvent>? OnModeChanged;
        event EventHandler<EventArgs>? OnDisconnect;

        //Task StartAsync(string host, int port, CancellationToken cancellationToken = default);
        Task StartAsync(Stream stream);//, CancellationToken cancellationToken = default);
        Task StopAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        Task<AzurClientData> ReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        AzurClientData Read(CancellationToken cancellationToken = default);

        void Write(string str, CancellationToken cancellationToken = default);
        void Write(ICommand command, CancellationToken cancellationToken = default);
        void Write(byte[] bytes, CancellationToken cancellationToken = default);
        void Write(AzurClientData data, CancellationToken cancellationToken = default);

        void WriteAll(IEnumerable<AzurClientData> data, CancellationToken cancellationToken = default);
        void WriteAll(IEnumerable<ICommand> commands, CancellationToken cancellationToken = default);
    }
}