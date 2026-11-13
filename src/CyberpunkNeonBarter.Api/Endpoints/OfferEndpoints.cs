using CyberpunkNeonBarter.Api.DTOs;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Repositories;
using CyberpunkNeonBarter.Infrastructure.Services;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class OfferEndpoints
{
    public static IEndpointRouteBuilder MapOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offers").WithTags("Offers").RequireAuthorization();

        // 1. Створити оголошення (UC-01)
        group.MapPost("/", async (
            CreateOfferRequest request,
            OfferService offerService,
            CancellationToken ct) =>
        {
            try
            {
                // ТУТ ВИПРАВЛЕНО: передаємо дані з request
                var offer = await offerService.CreateOfferAsync(
                    request.UserId,
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
                    Description = offer.Description, // ДОДАНО
                    ExpiresAt = offer.ExpiresAt,     // ДОДАНО
                    Status = offer.Status.ToString(),
                    CreatedAt = offer.CreatedAtUtc
                };
                return Results.Created($"/api/offers/{offer.Id}", response);
            }
            catch (ArgumentException ex)
            {
                // Ловимо помилку бізнес-правила (наприклад, минула дата)
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreateOffer")
        .WithOpenApi();

        // 2. Отримати ВСІ оголошення
        group.MapGet("/", async (
            IOfferRepository repo,
            CancellationToken ct) =>
        {
            var offers = await repo.GetAllAsync(ct);

            var response = offers.Select(offer => new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Description = offer.Description, // ДОДАНО
                ExpiresAt = offer.ExpiresAt,     // ДОДАНО
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            });

            return Results.Ok(response);
        })
        .WithName("GetAllOffers")
        .WithOpenApi();

        // 3. Отримати за ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            IOfferRepository repo,
            CancellationToken ct) =>
        {
            var offer = await repo.GetByIdAsync(id, ct);

            if (offer is null)
                return Results.NotFound(new { message = "Оголошення не знайдено" });

            var response = new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Description = offer.Description, // ДОДАНО
                ExpiresAt = offer.ExpiresAt,     // ДОДАНО
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            };
            return Results.Ok(response);
        })
        .WithName("GetOfferById")
        .WithOpenApi();

        // 4. Деактивувати
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IOfferRepository repo,
            CancellationToken ct) =>
        {
            var offer = await repo.GetByIdAsync(id, ct);

            if (offer is null)
                return Results.NotFound(new { message = "Оголошення не знайдено" });

            offer.Status = OfferStatus.Cancelled;
            await repo.UpdateAsync(offer, ct);

            return Results.Ok(new { message = "Оголошення деактивовано", status = offer.Status.ToString() });
        })
        .WithName("DeleteOffer")
        .WithOpenApi();

        return app;
    }
}