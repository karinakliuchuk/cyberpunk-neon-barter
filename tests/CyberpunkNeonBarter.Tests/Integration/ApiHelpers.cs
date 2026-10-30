using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace CyberpunkNeonBarter.Tests.Integration;

public record TokenDto(string Token);

public static class ApiHelpers
{
    public static async Task<string> LoginAsync(this HttpClient c, Guid userId, string role = "Participant")
    {
        var r = await c.PostAsJsonAsync("/api/auth/login", new { userId, role });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await r.Content.ReadFromJsonAsync<TokenDto>())!.Token;
    }

    public static HttpClient WithBearer(this HttpClient c, string token)
    {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    /// <summary>Підроблений/зіпсований токен для перевірки відмов (NFR-05).</summary>
    public static string ForgeToken(
        string secret = ApiFactory.Secret,
        string issuer = ApiFactory.Issuer,
        string audience = ApiFactory.Audience,
        TimeSpan? lifetime = null,
        string role = "Participant")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken(
            issuer, audience,
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            },
            notBefore: DateTime.UtcNow.AddHours(-3),
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
