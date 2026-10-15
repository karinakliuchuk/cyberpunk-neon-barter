namespace CyberpunkNeonBarter.Domain.Enums;

/// <summary>Життєвий цикл оголошення обміну.</summary>
public enum OfferStatus
{
    /// <summary>Оголошення створене і доступне для пошуку циклу (UC-01, AC-02).</summary>
    Active = 0,

    /// <summary>Оголошення входить до знайденого циклу обміну.</summary>
    Matched = 1,

    /// <summary>Обмін за участі цього оголошення виконано.</summary>
    Completed = 2,

    /// <summary>Оголошення скасоване автором.</summary>
    Cancelled = 3
}
