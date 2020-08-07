using Microsoft.EntityFrameworkCore;

namespace AzLn.GameClient.CommandsLogger
{
    public sealed class CommandsLoggerDbContext : DbContext
    {
        public DbSet<DbCommandLogEntry> Commands { get; set; } = null!;

        public CommandsLoggerDbContext(DbContextOptions<CommandsLoggerDbContext> builder) : base(builder)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var commands = modelBuilder.Entity<DbCommandLogEntry>();
            commands.HasKey(uc => uc.Id);
        }
    }
}