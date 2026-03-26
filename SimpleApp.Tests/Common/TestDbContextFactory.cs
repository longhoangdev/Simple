using Microsoft.EntityFrameworkCore;
using SimpleApp.Persistence.Contexts;

namespace SimpleApp.Tests.Common;

public static class TestDbContextFactory
{
    public static DataContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}
