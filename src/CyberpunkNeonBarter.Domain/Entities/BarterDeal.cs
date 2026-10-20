using CyberpunkNeonBarter.Domain.Enums;

namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>Угода: знайдений замкнений 3-сторонній цикл обміну A → B → C → A (FR-06…FR-08).</summary>
public class BarterDeal
{
    /// <summary>У v1.0 цикл складається рівно з трьох учасників.</summary>
    public const int CycleSize = 3;

    public Guid Id { get; set; }

    public DealStatus Status { get; set; } = DealStatus.Proposed;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>Учасники циклу. Зв'язок 1 → 3 (рівно три кроки передачі).</summary>
    public ICollection<BarterParticipant> Participants { get; set; } = new List<BarterParticipant>();

    /// <summary>Чи містить угода рівно три учасники.</summary>
    public bool HasValidCycleSize => Participants.Count == CycleSize;
}
