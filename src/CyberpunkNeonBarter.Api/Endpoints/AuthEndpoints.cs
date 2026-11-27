using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Infrastructure.Data;

namespace CyberpunkNeonBarter.Api.Endpoints;

public class RegisterRequest
{
    public string Callsign { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Callsign { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, BarterDbContext db, CancellationToken ct) =>
        {
            if (await db.Users.AnyAsync(u => u.Callsign == request.Callsign, ct))
                return Results.Conflict(new { message = "Користувач з таким позивним вже існує" });

            var user = new User
            {
                Id = Guid.NewGuid(),
                Callsign = request.Callsign,
                Contact = request.Contact,
                PasswordHash = HashPassword(request.Password),
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { userId = user.Id });
        })
        .WithName("Register")
        .WithOpenApi();

        group.MapPost("/login", async (LoginRequest request, BarterDbContext db, IConfiguration config, CancellationToken ct) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Callsign == request.Callsign, ct);

            if (user is null || user.PasswordHash != HashPassword(request.Password))
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Participant")
            };

            var jwtSettings = config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new { token = tokenString });
        })
        .WithName("Login")
        .WithOpenApi();

        return app;
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}