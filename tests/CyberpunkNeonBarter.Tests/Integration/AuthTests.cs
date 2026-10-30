using System.IdentityModel.Tokens.Jwt;
using System.Net;
using FluentAssertions;

namespace CyberpunkNeonBarter.Tests.Integration;

// NFR-05 (401) + перевірка JWT-авторизації з Program.cs / AuthEndpoints.cs
public class AuthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public AuthTests(ApiFactory f) => _f = f;

    private static string UnknownOfferUrl => $"/api/offers/{Guid.NewGuid()}";

    [Theory]
    [InlineData("Participant")]
    [InlineData("Admin")]
    public async Task Login_ReturnsJwt_WithUserIdAndRole(string role)
    {
        var userId = Guid.NewGuid();

        var token = await _f.CreateClient().LoginAsync(userId, role);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Value == role);
        jwt.Issuer.Should().Be(ApiFactory.Issuer);
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var r = await _f.CreateClient().GetAsync(UnknownOfferUrl);
        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_PassesAuth()
    {
        // Контрольний тест: токен, підписаний правильним ключем, приймається (404, а не 401)
        var client = _f.CreateClient().WithBearer(ApiHelpers.ForgeToken());
        var r = await client.GetAsync(UnknownOfferUrl);
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static IEnumerable<object[]> BadTokens()
    {
        yield return new object[] { "неправильний ключ",   ApiHelpers.ForgeToken(secret: "ANOTHER_SECRET_KEY_LONG_ENOUGH_FOR_HS256_ALGORITHM_0987") };
        yield return new object[] { "неправильний issuer",  ApiHelpers.ForgeToken(issuer: "Evil") };
        yield return new object[] { "неправильний audience", ApiHelpers.ForgeToken(audience: "Nobody") };
        yield return new object[] { "прострочений токен",   ApiHelpers.ForgeToken(lifetime: TimeSpan.FromHours(-2)) };
        yield return new object[] { "сміття замість JWT",   "not.a.jwt" };
    }

    [Theory]
    [MemberData(nameof(BadTokens))]
    public async Task ProtectedEndpoint_WithBadToken_Returns401(string name, string token)
    {
        var client = _f.CreateClient().WithBearer(token);

        var r = await client.GetAsync(UnknownOfferUrl);

        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized, because: name);
    }
}
