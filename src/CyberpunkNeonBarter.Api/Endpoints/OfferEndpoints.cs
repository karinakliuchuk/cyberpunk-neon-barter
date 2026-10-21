using CyberpunkNeonBarter.Api.DTOs;
using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class OfferEndpoints
{
    public static IEndpointRouteBuilder MapOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offers").WithTags("Offers").RequireAuthorization();

        // 1. Створити оголошення (UC-01)
        group.MapPost("/", async (
            CreateOfferRequest request, 
            BarterDbContext db, 
            CancellationToken ct) =>
        {
            var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId, ct);
            if (!userExists)
                return Results.BadRequest(new { message = "Користувача з таким ID не існує" });

            // Використовуємо метод Create, який написав Ян
            var offer = Offer.Create(request.UserId, request.Type, request.Category, request.Quantity);

            db.Offers.Add(offer);
            await db.SaveChangesAsync(ct);

            var response = new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            };

            return Results.Created($"/api/offers/{offer.Id}", response);
        })
        .WithName("CreateOffer")
        .WithOpenApi();

        // 2. Отримати оголошення за ID
        group.MapGet("/{id:guid}", async (
            Guid id, 
            BarterDbContext db, 
            CancellationToken ct) =>
        {
            var offer = await db.Offers.FindAsync(new object[] { id }, ct);
            
            if (offer is null)
                return Results.NotFound(new { message = "Оголошення не знайдено" });

            var response = new OfferResponse
            {
                Id = offer.Id,
                UserId = offer.UserId,
                Type = offer.Type.ToString(),
                Category = offer.Category.ToString(),
                Quantity = offer.Quantity,
                Status = offer.Status.ToString(),
                CreatedAt = offer.CreatedAtUtc
            };

            return Results.Ok(response);
        })
        .WithName("GetOfferById")
        .WithOpenApi();

        // 3. Деактивувати оголошення
        group.MapDelete("/{id:guid}", async (
            Guid id, 
            BarterDbContext db, 
            CancellationToken ct) =>
        {
            var offer = await db.Offers.FindAsync(new object[] { id }, ct);
            
            if (offer is null)
                return Results.NotFound(new { message = "Оголошення не знайдено" });

            offer.Status = OfferStatus.Cancelled;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Оголошення деактивовано", status = offer.Status.ToString() });
        })
        .WithName("DeleteOffer")
        .WithOpenApi();

        return app;
    }
}