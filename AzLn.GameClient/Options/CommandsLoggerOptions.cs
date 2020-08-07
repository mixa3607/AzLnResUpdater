using AzLn.GameClient.CommandsLogger;

namespace AzLn.GameClient.Options
{
    public interface ICommandsLoggerOptions
    {
        bool Enable { get; }
        bool Validate { get; }
        CommandsLoggerDbContext? DbContext { get; }
        int PreWriteBufferLen { get; }
    }

    public class CommandsLoggerOptions : ICommandsLoggerOptions
    {
        public bool Enable => DbContext != null;
        public bool Validate { get; set; }
        public CommandsLoggerDbContext? DbContext { get; set; }
        public int PreWriteBufferLen { get; set; }
    }
}