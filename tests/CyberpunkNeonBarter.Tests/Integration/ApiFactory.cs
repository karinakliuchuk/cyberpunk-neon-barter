using System.Net.Http.Headers;
using CyberpunkNeonBarter.Api.Endpoints;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CyberpunkNeonBarter.Tests.Integration;

// WebApplicationFactory<T> потрібен будь-який клас із збірки Api. Program у вас top-level,
// тому беремо LoginRequest — так не доводиться чіпати Program.cs.
public class ApiFactory : WebApplicationFactory<LoginRequest>
{
    public const string Secret = "TEST_SECRET_KEY_FOR_CYBERPUNK_BARTER_TESTS_1234567890";
    public const string Issuer = "CyberpunkNeonBarter";
    public const string Audience = "CyberpunkUsers";

    private readonly string _dbName = Guid.NewGuid().ToString();

    static ApiFactory()
    {
        // Program.cs читає конфіг одразу при старті й падає без цих значень,
        // тому передаємо їх через змінні середовища (appsettings.json їх не містить).
        Environment.SetEnvironmentVariable("ConnectionStrings__Barter", "Host=unused;Database=unused");
        Environment.SetEnvironmentVariable("JwtSettings__Secret", Secret);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", Issuer);
        Environment.SetEnvironmentVariable("JwtSettings__Audience", Audience);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Прибираємо реєстрацію Npgsql (у EF Core 9 їх кілька) і ставимо InMemory
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(BarterDbContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GenericTypeArguments.Contains(typeof(BarterDbContext)))).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<BarterDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>Клієнт з реальним JWT, отриманим через /api/auth/login.</summary>
    public async Task<HttpClient> ClientAsync(Guid? userId = null, string role = "Participant")
    {
        var client = CreateClient();
        var token = await client.LoginAsync(userId ?? Guid.NewGuid(), role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public BarterDbContext Db() => Services.CreateScope().ServiceProvider.GetRequiredService<BarterDbContext>();
}
