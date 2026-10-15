namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>Запис журналу змін: хто, що і коли зробив із сутністю системи.</summary>
public class AuditLog
{
    public Guid Id { get; set; }

    /// <summary>Хто виконав дію. Порожнє для системних дій (наприклад, пошук циклу).</summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Назва сутності, наприклад "Offer" або "BarterDeal".</summary>
    public string EntityName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>Дія, наприклад "Created", "StatusChanged".</summary>
    public string Action { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTime TimestampUtc { get; set; }
}
