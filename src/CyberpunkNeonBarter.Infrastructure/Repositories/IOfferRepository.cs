using CyberpunkNeonBarter.Domain.Entities;

namespace CyberpunkNeonBarter.Infrastructure.Repositories;

public interface IOfferRepository
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken ct);
    Task<Offer> AddAsync(Offer offer, CancellationToken ct);
    Task<Offer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Offer offer, CancellationToken ct);
}