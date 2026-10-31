using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace CyberpunkNeonBarter.Tests.Integration;

// NFR-05: коректні коди помилок для /api/offers (OfferEndpoints.cs)
public class OfferEndpointsErrorTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public OfferEndpointsErrorTests(ApiFactory f) => _f = f;

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var r = await _f.CreateClient().PostAsJsonAsync("/api/offers",
            new { userId = Guid.NewGuid(), type = 0, category = 0, quantity = 1 });
        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ForUnknownUser_Returns400WithMessage()
    {
        var client = await _f.ClientAsync();

        var r = await client.PostAsJsonAsync("/api/offers",
            new { userId = Guid.NewGuid(), type = 0, category = 0, quantity = 1 });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("message", out var msg).Should().BeTrue();
        msg.GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Create_WithInvalidEnumValue_Returns400()
    {
        var client = await _f.ClientAsync();

        var r = await client.PostAsJsonAsync("/api/offers",
            new { userId = Guid.NewGuid(), type = "NotAType", category = "NotACategory", quantity = 1 });

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithMalformedJson_Returns400()
    {
        var client = await _f.ClientAsync();

        var r = await client.PostAsync("/api/offers",
            new StringContent("{bad json", Encoding.UTF8, "application/json"));

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithWrongContentType_Returns4xx()
    {
        var client = await _f.ClientAsync();

        var r = await client.PostAsync("/api/offers", new StringContent("hello", Encoding.UTF8, "text/plain"));

        ((int)r.StatusCode).Should().BeInRange(400, 499);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404WithMessage()
    {
        var client = await _f.ClientAsync();

        var r = await client.GetAsync($"/api/offers/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("message", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_NotAGuid_Returns404()
    {
        var client = await _f.ClientAsync();
        var r = await client.GetAsync("/api/offers/not-a-guid");
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var client = await _f.ClientAsync();
        var r = await client.DeleteAsync($"/api/offers/{Guid.NewGuid()}");
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        var r = await _f.CreateClient().DeleteAsync($"/api/offers/{Guid.NewGuid()}");
        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
