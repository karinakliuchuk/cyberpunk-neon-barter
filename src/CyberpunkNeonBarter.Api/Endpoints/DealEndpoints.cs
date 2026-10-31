using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class DealEndpoints
{
    public static IEndpointRouteBuilder MapDealEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Підтвердження участі в угоді (UC-03)
        app.MapPost("/api/deals/{dealId}/confirm", async (
            Guid dealId, 
            Guid userId, // В реальності береться з JWT-токена
            BarterDbContext db, 
            CancellationToken ct) =>
        {
            // Завантажуємо угоду разом з учасниками та їхніми оголошеннями "віддаю"
            var deal = await db.BarterDeals
                .Include(d => d.Participants)
                .ThenInclude(p => p.GiveOffer)
                .FirstOrDefaultAsync(d => d.Id == dealId, ct);

            if (deal is null)
                return Results.NotFound(new { message = "Угоду не знайдено" });

            // Ян використовує статус Proposed замість PendingConfirmations
            if (deal.Status != DealStatus.Proposed)
                return Results.Conflict(new { message = "Угода вже не очікує підтвердження" });

            var participant = deal.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant is null)
                return Results.BadRequest(new { message = "Користувач не є учасником цієї угоди" });

            if (participant.TransferConfirmed)
                return Results.Ok(new { message = "Ви вже підтвердили участь" });

            // Фіксація згоди
            participant.TransferConfirmed = true;

            // Перевірка, чи всі підтвердили
            if (deal.Participants.All(p => p.TransferConfirmed))
            {
                
                deal.Status = DealStatus.InProgress; 
                
                // Оновлюємо статуси оголошень на "Matched" (знайдено пару)
                foreach (var p in deal.Participants)
                {
                    if (p.GiveOffer != null) p.GiveOffer.Status = OfferStatus.Matched;
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

            // Скасовуємо угоду
            deal.Status = DealStatus.Cancelled;

            // Повертаємо оголошення в статус Active
            foreach (var p in deal.Participants)
            {
                if (p.GiveOffer != null) p.GiveOffer.Status = OfferStatus.Active;
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { dealId = deal.Id, status = deal.Status.ToString() });
        })
        .WithName("CancelDeal")
        .WithOpenApi();

        return app;
    }
}
