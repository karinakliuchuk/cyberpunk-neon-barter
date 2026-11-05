using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Infrastructure.Repositories;

public class OfferRepository(BarterDbContext db) : IOfferRepository
{
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken ct) =>
        await db.Users.AnyAsync(u => u.Id == userId, ct);

    public async Task<Offer> AddAsync(Offer offer, CancellationToken ct)
    {
        db.Offers.Add(offer);
        await db.SaveChangesAsync(ct);
        return offer;
    }

    public async Task<Offer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Offers.FindAsync([id], ct);

    public async Task UpdateAsync(Offer offer, CancellationToken ct)
    {
        db.Offers.Update(offer);
        await db.SaveChangesAsync(ct);
    }
}