using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CyberpunkNeonBarter.Tests.Integration;

public class CycleAndHealthEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public CycleAndHealthEndpointTests(ApiFactory f) => _f = f;

    [Fact]
    public async Task Health_Returns200()
    {
        var r = await _f.CreateClient().GetAsync("/health");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().Should().Be("healthy");
    }

    // NFR-05: коли циклу немає — 409 і код cycle_not_found (CycleEndpoints.cs)
    [Fact]
    public async Task FindCycle_OnEmptyDatabase_Returns409_CycleNotFound()
    {
        using var factory = new ApiFactory();   // чиста порожня БД
        var client = factory.CreateClient();

        var r = await client.PostAsync("/api/cycles/find", null);

        r.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("cycle_not_found");
    }
}
