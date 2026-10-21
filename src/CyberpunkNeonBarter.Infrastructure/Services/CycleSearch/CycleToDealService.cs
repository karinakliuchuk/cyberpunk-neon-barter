using Microsoft.EntityFrameworkCore;
using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;

namespace CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;

/// <summary>
/// Перетворює знайдений цикл на BarterDeal + 3 BarterParticipant + AuditLog.
/// </summary>
public sealed class CycleToDealService(BarterDbContext db)
{
    public async Task<BarterDeal> CreateDealAsync(FoundCycle cycle, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var deal = new BarterDeal
        {
            Id = Guid.NewGuid(),
            Status = DealStatus.Proposed,
            CreatedAtUtc = now
        };

        // Крок 1: A віддає GiveA → B (отримує NeedA)
        deal.Participants.Add(new BarterParticipant
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            UserId = cycle.ParticipantA,
            StepOrder = 1,
            GiveOfferId = cycle.GiveOfferA,
            NeedOfferId = cycle.NeedOfferA
        });

        // Крок 2: B віддає GiveB → C
        deal.Participants.Add(new BarterParticipant
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            UserId = cycle.ParticipantB,
            StepOrder = 2,
            GiveOfferId = cycle.GiveOfferB,
            NeedOfferId = cycle.NeedOfferB
        });

        // Крок 3: C віддає GiveC → A
        deal.Participants.Add(new BarterParticipant
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            UserId = cycle.ParticipantC,
            StepOrder = 3,
            GiveOfferId = cycle.GiveOfferC,
            NeedOfferId = cycle.NeedOfferC
        });

        db.BarterDeals.Add(deal);

        // Резервуємо всі три Give-оголошення (OfferStatus.Matched у моделі)
        var giveIds = new[] { cycle.GiveOfferA, cycle.GiveOfferB, cycle.GiveOfferC };
        var giveOffers = await db.Offers
            .Where(o => giveIds.Contains(o.Id))
            .ToListAsync(ct);

        foreach (var offer in giveOffers)
            offer.Status = OfferStatus.Matched;

        // Аудит
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(BarterDeal),
            EntityId = deal.Id,
            Action = "Created",
            Details = $"3-cycle A={cycle.ParticipantA} B={cycle.ParticipantB} C={cycle.ParticipantC}",
            TimestampUtc = now
        });

        await db.SaveChangesAsync(ct);
        return deal;
    }
}