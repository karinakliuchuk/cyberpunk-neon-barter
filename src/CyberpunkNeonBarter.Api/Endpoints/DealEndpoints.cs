using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class DealEndpoints
{
    public static IEndpointRouteBuilder MapDealEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Підтвердження участі в угоді (UC-03)
        app.MapPost("/api/deals/{dealId}/confirm", async (
            Guid dealId,
            Guid userId,
            BarterDbContext db,
            IOfferRepository offerRepo,
            CancellationToken ct) =>
        {
            var deal = await db.BarterDeals
                .Include(d => d.Participants)
                .ThenInclude(p => p.GiveOffer)
                .FirstOrDefaultAsync(d => d.Id == dealId, ct);

            if (deal is null)
                return Results.NotFound(new { message = "Угоду не знайдено" });

            if (deal.Status != DealStatus.Proposed)
                return Results.Conflict(new { message = "Угода вже не очікує підтвердження" });

            var participant = deal.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant is null)
                return Results.BadRequest(new { message = "Користувач не є учасником цієї угоди" });

            if (participant.TransferConfirmed)
                return Results.Ok(new { message = "Ви вже підтвердили участь" });

            participant.TransferConfirmed = true;

            if (deal.Participants.All(p => p.TransferConfirmed))
            {
                deal.Status = DealStatus.InProgress;

                foreach (var p in deal.Participants)
                {
                    if (p.GiveOffer != null)
                    {
                        p.GiveOffer.Status = OfferStatus.Matched;
                        await offerRepo.UpdateAsync(p.GiveOffer, ct);
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                dealId = deal.Id,
                status = deal.Status.ToString(),
                message = deal.Status == DealStatus.InProgress ? "Всі підтвердили! Угода активна." : "Згоду зафіксовано. Очікуємо інших."
            });
        })
        .WithName("ConfirmDeal")
        .WithOpenApi();

        // 2. Скасування угоди (UC-04)
        app.MapPost("/api/deals/{dealId}/cancel", async (
            Guid dealId,
            Guid userId,
            BarterDbContext db,
            IOfferRepository offerRepo,
            CancellationToken ct) =>
        {
            var deal = await db.BarterDeals
                .Include(d => d.Participants)
                .ThenInclude(p => p.GiveOffer)
                .FirstOrDefaultAsync(d => d.Id == dealId, ct);

            if (deal is null) return Results.NotFound();
            var participant = deal.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant is null) return Results.Forbid();

            if (deal.Status == DealStatus.Completed || deal.Status == DealStatus.Cancelled)
                return Results.Conflict(new { message = "Угода вже завершена або скасована" });

            deal.Status = DealStatus.Cancelled;

            foreach (var p in deal.Participants)
            {
                if (p.GiveOffer != null)
                {
                    p.GiveOffer.Status = OfferStatus.Active;
                    await offerRepo.UpdateAsync(p.GiveOffer, ct);
                }
            }
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { dealId = deal.Id, status = deal.Status.ToString() });
        })
        .WithName("CancelDeal")
        .WithOpenApi();

        return app;
    }
}