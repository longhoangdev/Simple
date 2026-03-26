using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SimpleApp.LogServer.Persistence;

public sealed class LogDbContextFactory : IDesignTimeDbContextFactory<LogDbContext>
{
    public LogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LogDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=simple-log-db;Username=postgres;Password=postgres",
                o => o.MigrationsAssembly(typeof(LogDbContext).Assembly.FullName))
            .Options;

        return new LogDbContext(options);
    }
}
