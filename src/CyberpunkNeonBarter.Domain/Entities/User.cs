namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>Учасник ринку: картка з позивним і контактом (FR-01).</summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Позивний або ім'я учасника.</summary>
    public string Callsign { get; set; } = string.Empty;

    /// <summary>Базовий контакт (телефон, месенджер тощо).</summary>
    public string Contact { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Оголошення автора. Зв'язок 1 → 0..*.</summary>
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    /// <summary>Участі в угодах. Зв'язок 1 → 0..*.</summary>
    public ICollection<BarterParticipant> Participations { get; set; } = new List<BarterParticipant>();
}
