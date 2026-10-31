using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CyberpunkNeonBarter.Infrastructure.Data;

public sealed class BarterDbContextFactory : IDesignTimeDbContextFactory<BarterDbContext>
{
    public BarterDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Barter")
            ?? "Host=localhost;Port=5432;Database=CyberpunkNeonBarter;Username=postgres;Password=cA7zyV1sI5wkQ3d";

        var options = new DbContextOptionsBuilder<BarterDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(
                    typeof(BarterDbContext).Assembly.FullName);
            })
            .Options;

        return new BarterDbContext(options);
    }
}