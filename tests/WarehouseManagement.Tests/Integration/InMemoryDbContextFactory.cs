using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Infrastructure.Persistence;

namespace WarehouseManagement.Tests.Integration;

public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create(string dbName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
