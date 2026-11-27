namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>Учасник ринку: картка з позивним і контактом (FR-01).</summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Позивний або ім'я учасника.</summary>
    public string Callsign { get; set; } = string.Empty;

    /// <summary>Базовий контакт (телефон, месенджер тощо).</summary>
    public string Contact { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля для входу (додано в спринті ЛР №8). Сам пароль ніколи не зберігається.
    /// Може бути null для користувачів, створених до появи реєстрації: такі записи
    /// залишаються коректними, але увійти за паролем вони не можуть.
    /// </summary>
    public string? PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Оголошення автора. Зв'язок 1 → 0..*.</summary>
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    /// <summary>Участі в угодах. Зв'язок 1 → 0..*.</summary>
    public ICollection<BarterParticipant> Participations { get; set; } = new List<BarterParticipant>();
}
