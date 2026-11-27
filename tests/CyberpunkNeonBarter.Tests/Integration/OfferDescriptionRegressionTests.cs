using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CyberpunkNeonBarter.Tests.Integration;

/// <summary>
/// Регресійні тести контракту API (ЛР №8): поле <c>description</c> оголошення має повертатися
/// у відповідях API. Фіксують контракт, на який спирається вивід колонки «Опис» у таблиці
/// оголошень (index.html), щоб згодом його випадково не втратили.
/// </summary>
public sealed class OfferDescriptionRegressionTests : IClassFixture<OfferEndpointsTests.BarterApiFactory>
{
    // Той самий користувач, якого створює BarterApiFactory у OfferEndpointsTests.
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private const int ValidType = 0;       // OfferType.Give
    private const int ValidCategory = 4;   // ResourceCategory.Energy
    private const string Description = "Опис для регресійного тесту";

    private readonly OfferEndpointsTests.BarterApiFactory _factory;

    public OfferDescriptionRegressionTests(OfferEndpointsTests.BarterApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetOfferById_ReturnsDescriptionOfCreatedOffer()
    {
        // Підготовка
        var client = await CreateAuthorizedClientAsync();
        var id = await CreateOfferAsync(client, description: Description);

        // Дія
        var response = await client.GetAsync($"/api/offers/{id}");

        // Перевірка
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(Description, FindProperty(json.RootElement, "description").GetString());
    }

    [Fact]
    public async Task GetOffers_List_ReturnsDescriptionOfCreatedOffer()
    {
        // Підготовка
        var client = await CreateAuthorizedClientAsync();
        var id = await CreateOfferAsync(client, description: Description);

        // Дія
        var response = await client.GetAsync("/api/offers");

        // Перевірка
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var created = EnumerateOffers(json.RootElement)
            .Single(o => FindProperty(o, "id").GetGuid() == id);
        Assert.Equal(Description, FindProperty(created, "description").GetString());
    }

    [Fact]
    public async Task GetOfferById_WithoutDescription_ReturnsNoDescription()
    {
        // Підготовка
        var client = await CreateAuthorizedClientAsync();
        var id = await CreateOfferAsync(client, description: null);

        // Дія
        var response = await client.GetAsync($"/api/offers/{id}");

        // Перевірка: поле або відсутнє, або null (опис необов'язковий)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var description = TryFindProperty(json.RootElement, "description");
        Assert.True(description is null || description.Value.ValueKind == JsonValueKind.Null,
            "Для оголошення без опису поле description має бути відсутнім або null.");
    }

    // ---------- допоміжні методи ----------

    private static async Task<Guid> CreateOfferAsync(HttpClient client, string? description)
    {
        var response = await client.PostAsJsonAsync("/api/offers", new
        {
            userId = TestUserId,
            type = ValidType,
            category = ValidCategory,
            quantity = 4,
            description
        });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"Створення оголошення: статус {(int)response.StatusCode}. Тіло відповіді: {body}");

        using var json = JsonDocument.Parse(body);
        return FindProperty(json.RootElement, "id").GetGuid();
    }

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

    /// <summary>Відповідь може бути масивом або обгорткою з масивом усередині.</summary>
    private static List<JsonElement> EnumerateOffers(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().ToList();

        foreach (var property in root.EnumerateObject())
            if (property.Value.ValueKind == JsonValueKind.Array)
                return property.Value.EnumerateArray().ToList();

        throw new InvalidOperationException("У відповіді не знайдено списку оголошень.");
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
}
