using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WarehouseManagement.Infrastructure.Persistence;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            @"Data Source=DESKTOP-CLJC02\TRUNGLE;
              Initial Catalog=WarehouseManagementDb;
              Integrated Security=True;
              TrustServerCertificate=True;
              Connection Timeout=180");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}