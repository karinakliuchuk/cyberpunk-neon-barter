using System.Security.Claims;
using CyberpunkNeonBarter.Api.DTOs;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Repositories;
using CyberpunkNeonBarter.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class OfferEndpoints
{
    public static IEndpointRouteBuilder MapOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offers").WithTags("Offers").RequireAuthorization();

        // 1. Створити оголошення
        group.MapPost("/", async (
            CreateOfferRequest request,
            ClaimsPrincipal userClaim,
            OfferService offerService,
            CancellationToken ct) =>
        {
            var userIdStr = userClaim.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            try
            {
                var offer = await offerService.CreateOfferAsync(
                    userId,
                    request.Type,
                    request.Category,
                    request.Quantity,
                    request.Description,
                    request.ExpiresAt,
                    ct);

                if (offer is null)
                    return Results.BadRequest(new { message = "Користувача з таким ID не існує" });

                var response = new OfferResponse
                {
                    Id = offer.Id,
                    UserId = offer.UserId,
                    Type = offer.Type.ToString(),
                    Category = offer.Category.ToString(),
                    Quantity = offer.Quantity,
                    Description = offer.Description,
                    ExpiresAt = offer.ExpiresAt,
                    Status = offer.Status.ToString(),
                    CreatedAt = offer.CreatedAtUtc
                };
                return Results.Created($"/api/offers/{offer.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }).WithName("CreateOffer").WithOpenApi();

        // 2. Отримати власні оголошення (NEW)
        group.MapGet("/my", async (
            BarterDbContext db,
            ClaimsPrincipal userClaim,
            CancellationToken ct) =>
        {
            var userIdStr = userClaim.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var offers = await db.Offers.AsNoTracking().Where(o => o.UserId == userId).ToListAsync(ct);
            var response = offers.Select(offer => new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Description = offer.Description,
                ExpiresAt = offer.ExpiresAt,
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            });
            return Results.Ok(response);
        }).WithName("GetMyOffers").WithOpenApi();

        // 3. Отримати ВСІ оголошення
        group.MapGet("/", async (IOfferRepository repo, CancellationToken ct) =>
        {
            var offers = await repo.GetAllAsync(ct);
            var response = offers.Select(offer => new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Description = offer.Description,
                ExpiresAt = offer.ExpiresAt,
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            });
            return Results.Ok(response);
        }).WithName("GetAllOffers").WithOpenApi();

        // 4. Отримати за ID
        group.MapGet("/{id:guid}", async (Guid id, IOfferRepository repo, CancellationToken ct) =>
        {
            var offer = await repo.GetByIdAsync(id, ct);
            if (offer is null) return Results.NotFound(new { message = "Оголошення не знайдено" });

            var response = new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Description = offer.Description,
                ExpiresAt = offer.ExpiresAt,
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            };
            return Results.Ok(response);
        }).WithName("GetOfferById").WithOpenApi();

        // 5. Деактивувати
        group.MapDelete("/{id:guid}", async (Guid id, IOfferRepository repo, CancellationToken ct) =>
        {
            var offer = await repo.GetByIdAsync(id, ct);
            if (offer is null) return Results.NotFound(new { message = "Оголошення не знайдено" });

            offer.Status = OfferStatus.Cancelled;
            await repo.UpdateAsync(offer, ct);
            return Results.Ok(new { message = "Оголошення деактивовано", status = offer.Status.ToString() });
        }).WithName("DeleteOffer").WithOpenApi();

        return app;
    }
}