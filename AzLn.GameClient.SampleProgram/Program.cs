using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AzLn.GameClient.Options;
using AzLn.Protocol.P10;
using NLog;
using EventArgs = System.EventArgs;

namespace AzLn.GameClient.SampleProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var connOpts = new ConnectionOptions()
            {
                InitBufferLen = 40960,
                ReadBatchLen = 16384,
                BufferResizeStep = 8192
            };
            using var client = new AzurClient(connOpts, LogManager.GetLogger("client"));
            using var tcp = new TcpClient("blhxusgate.yo-star.com", 80);

            void Disconnect(object? sender, EventArgs eventArgs) => logger.Debug("Disconnected");
            void ModeChanged(object? sender, ClientModeChangedEvent modeArgs) => logger.Debug($"Switch to {modeArgs.ClientMode} mode");
            client.OnDisconnect += Disconnect;
            client.OnModeChanged += ModeChanged;
            //var client = new AzurClient.AzurClient("127.0.0.1", 25000, connOpts);
            //var tcp = new TcpClient("blhxusgate.yo-star.com", 80);
            client.StartAsync(tcp.GetStream()).Wait();
            var cs10800 = new Cs10800()
            {
                Platform = "0",
                State = 21,
                Index = 0
            };
            client.Write(cs10800);
            //client.Write(File.ReadAllText("./c.txt"));
            var rd = client.ReadAsync().Result;
            client.OnDisconnect -= Disconnect;
            client.OnModeChanged -= ModeChanged;
            client.StopAsync().Wait();

            //Task.Delay(Timeout.Infinite).Wait();
        }
    }
}