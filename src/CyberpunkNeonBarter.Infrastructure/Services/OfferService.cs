using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Domain.Rules;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Repositories;

namespace CyberpunkNeonBarter.Infrastructure.Services;

public class OfferService(BarterDbContext db, IOfferRepository repo)
{
    public async Task<Offer?> CreateOfferAsync(
        Guid userId, 
        OfferType type, 
        ResourceCategory category, 
        int quantity, 
        string? description = null, 
        DateTime? expiresAt = null, 
        CancellationToken ct = default)
    {
        // 1. Перевірка наявності користувача
        var userExists = await repo.UserExistsAsync(userId, ct);
        if (!userExists)
            return null;

        // 2. Валідація бізнес-правил
        if (!OfferRule.IsValid(expiresAt))
            throw new ArgumentException("Некоректний термін дії оголошення.");

        // 3. Атомарне збереження з транзакцією
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var offer = Offer.Create(userId, type, category, quantity, description, expiresAt);
            await repo.AddAsync(offer, ct);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityName = nameof(Offer),
                EntityId = offer.Id,
                Action = "Created",
                Details = $"Створено оголошення типу {type} на ресурс {category}.",
                TimestampUtc = DateTime.UtcNow
            };
            db.AuditLogs.Add(auditLog);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return offer;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}