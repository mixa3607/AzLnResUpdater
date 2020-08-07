using AzLn.GameClient.CommandsLogger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AzLn.GameClient.SampleProgram
{
    //for dotnet-ef cmd
    public class CommandsLoggerContextFactory : IDesignTimeDbContextFactory<CommandsLoggerDbContext>
    {
        public CommandsLoggerDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CommandsLoggerDbContext>()
                .UseSqlite("Data Source=raw_commands.sqlite", b => b.MigrationsAssembly(GetType().Assembly.FullName));
            return new CommandsLoggerDbContext(builder.Options);
        }
    }
}