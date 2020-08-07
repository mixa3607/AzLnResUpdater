using System.Threading.Tasks;

namespace AzLn.GameClient.CommandsLogger
{
    public interface ICommandsLogger
    {
        Task LogReceiveCommandsAsync(byte[] commandBytes, ushort commandId, ushort idx);
        Task LogSendCommandAsync(byte[] commandBytes, ushort commandId, ushort idx);
    }
}