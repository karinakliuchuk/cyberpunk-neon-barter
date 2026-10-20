using CyberpunkNeonBarter.Domain.Enums;

namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>Оголошення обміну «віддаю» або «потребую» (UC-01, FR-02…FR-05).</summary>
public class Offer
{
    public Guid Id { get; set; }

    /// <summary>Автор оголошення. Обов'язковий зв'язок: кожне оголошення має одного автора.</summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public OfferType Type { get; set; }

    public ResourceCategory Category { get; set; }

    /// <summary>Кількість у стандартних цілих одиницях («пакетах»), більша за нуль (FR-04).</summary>
    public int Quantity { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Active;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Створює коректне оголошення зі статусом «Активне».
    /// Предметні правила з SRS: категорія зі списку (FR-03), кількість ціла і &gt; 0 (FR-04),
    /// автор вказаний (FR-05).
    /// </summary>
    public static Offer Create(Guid userId, OfferType type, ResourceCategory category, int quantity)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Потрібно вказати автора оголошення.", nameof(userId));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), "Недопустимий тип оголошення.");

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category), "Категорія не входить до фіксованого списку.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Кількість має бути цілим числом більшим за нуль.");

        return new Offer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Category = category,
            Quantity = quantity,
            Status = OfferStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
