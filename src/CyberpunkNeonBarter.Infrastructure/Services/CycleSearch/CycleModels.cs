using CyberpunkNeonBarter.Domain.Entities;

namespace CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;

/// <summary>Одне ребро графа: X віддає GiveOffer і це покриває NeedOffer Y.</summary>
public sealed record Edge(
    Guid FromUserId,
    Guid ToUserId,
    Offer GiveOffer,
    Offer NeedOffer);

/// <summary>Кандидат циклу з трьох користувачів та їхніх оголошень.</summary>
internal sealed record CycleCandidate(
    Guid A, Guid B, Guid C,
    Offer GiveA, Offer NeedA,
    Offer GiveB, Offer NeedB,
    Offer GiveC, Offer NeedC);

/// <summary>Результат пошуку, який віддається назовні.</summary>
public sealed record FoundCycle(
    Guid ParticipantA, Guid ParticipantB, Guid ParticipantC,
    Guid GiveOfferA, Guid GiveOfferB, Guid GiveOfferC,
    Guid NeedOfferA, Guid NeedOfferB, Guid NeedOfferC);