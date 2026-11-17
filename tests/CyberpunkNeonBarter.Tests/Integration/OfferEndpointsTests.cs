using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using Xunit;

namespace CyberpunkNeonBarter.Tests.Integration;

/// <summary>
/// Характеризаційні тести API оголошень (ЛР №7, спринт S3, задача №1).
/// Фіксують поточну поведінку сервера ДО рефакторингу клієнта (BarterApiClient):
/// створення, читання за списком і за Id, відмова на некоректні дані, доступ без токена.
/// Тести працюють з SQLite у пам'яті, тому не потребують запущеного PostgreSQL.
/// </summary>
public sealed class OfferEndpointsTests : IClassFixture<OfferEndpointsTests.BarterApiFactory>
{
    // Фіксований користувач: /api/auth/login не звертається до бази, а лише видає JWT для UserId,
    // тому користувач із цим Id має існувати в тестовій базі (його створює фабрика нижче).
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // Числові значення enum: API приймає їх незалежно від того, чи налаштовано рядкову серіалізацію.
    private const int ValidType = (int)CyberpunkNeonBarter.Domain.Enums.OfferType.Give;
    private const int ValidCategory = (int)CyberpunkNeonBarter.Domain.Enums.ResourceCategory.Energy;

    private readonly BarterApiFactory _factory;

    public OfferEndpointsTests(BarterApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateOffer_WithoutToken_IsRejected()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/offers", ValidOffer(quantity: 5));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOffer_ValidRequest_Succeeds()
    {
        var client = await CreateAuthorizedClientAsync();

        var response = await client.PostAsJsonAsync("/api/offers", ValidOffer(quantity: 5));

        // Зафіксований код успішного створення. Якщо тест покаже інший (наприклад, 200 OK),
        // підстав у Assert.Equal фактичний код: це і є поточна поведінка API.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOffer_ThenGetById_ReturnsSameOffer()
    {
        var client = await CreateAuthorizedClientAsync();
        var created = await client.PostAsJsonAsync("/api/offers", ValidOffer(quantity: 7));
        var id = await ReadIdAsync(created);

        var response = await client.GetAsync($"/api/offers/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(7, FindProperty(json.RootElement, "quantity").GetInt32());
        Assert.Equal("Характеризаційний тест", FindProperty(json.RootElement, "description").GetString());
    }

    [Fact]
    public async Task CreateOffer_ThenGetList_ContainsCreatedOffer()
    {
        var client = await CreateAuthorizedClientAsync();
        var created = await client.PostAsJsonAsync("/api/offers", ValidOffer(quantity: 3));
        var id = await ReadIdAsync(created);

        var response = await client.GetAsync("/api/offers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(id.ToString(), body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOffer_QuantityZero_ReturnsBadRequest()
    {
        var client = await CreateAuthorizedClientAsync();

        var response = await client.PostAsJsonAsync("/api/offers", ValidOffer(quantity: 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOfferById_UnknownId_ReturnsNotFound()
    {
        var client = await CreateAuthorizedClientAsync();

        var response = await client.GetAsync($"/api/offers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- допоміжні методи ----------

    private static object ValidOffer(int quantity) => new
    {
        userId = TestUserId,   // якщо API бере автора з токена, зайве поле просто ігнорується
        type = ValidType,
        category = ValidCategory,
        quantity,
        description = "Характеризаційний тест"
    };

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var client = _factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { userId = TestUserId, role = "Participant" });
        login.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = FindProperty(json.RootElement, "token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"Створення оголошення: статус {(int)response.StatusCode}. Тіло відповіді: {body}");
        using var json = JsonDocument.Parse(body);
        return FindProperty(json.RootElement, "id").GetGuid();
    }

    private static JsonElement FindProperty(JsonElement element, string name) =>
        TryFindProperty(element, name)
        ?? throw new InvalidOperationException($"У відповіді немає поля '{name}'.");

    private static JsonElement? TryFindProperty(JsonElement element, string name)
    {
        foreach (var property in element.EnumerateObject())
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        return null;
    }

    /// <summary>Піднімає API в пам'яті з SQLite замість PostgreSQL.</summary>
    public sealed class BarterApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();

            builder.ConfigureServices(services =>
            {
                // Прибираємо реєстрацію PostgreSQL. Інтерфейс IDbContextOptionsConfiguration<> в EF Core 9
                // є внутрішнім, тому шукаємо його за назвою типу.
                services.RemoveAll<DbContextOptions<BarterDbContext>>();
                var optionsConfigurations = services
                    .Where(d => d.ServiceType.IsGenericType
                                && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")
                                && d.ServiceType.GenericTypeArguments.Contains(typeof(BarterDbContext)))
                    .ToList();
                foreach (var descriptor in optionsConfigurations)
                    services.Remove(descriptor);

                services.AddDbContext<BarterDbContext>(o => o.UseSqlite(_connection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BarterDbContext>();
                db.Database.EnsureCreated();

                // Оголошення мають обов'язкового автора (зовнішній ключ), тому створюємо користувача.
                db.Set<User>().Add(new User
                {
                    Id = TestUserId,
                    Callsign = "TestRunner",
                    Contact = "test@example.com",
                    CreatedAtUtc = DateTime.UtcNow
                });
                db.SaveChanges();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _connection.Dispose();
        }
    }
}
