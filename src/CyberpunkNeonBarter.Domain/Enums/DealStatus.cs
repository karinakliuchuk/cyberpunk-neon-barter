namespace CyberpunkNeonBarter.Domain.Enums;

/// <summary>Статус виконання угоди (3-стороннього циклу), FR-08.</summary>
public enum DealStatus
{
    /// <summary>Цикл знайдено алгоритмом, учасники ще не підтвердили.</summary>
    Proposed = 0,

    /// <summary>Учасники підтвердили участь.</summary>
    Confirmed = 1,

    /// <summary>Передача ресурсів триває.</summary>
    InProgress = 2,

    /// <summary>Усі три передачі виконано.</summary>
    Completed = 3,

    /// <summary>Угоду скасовано.</summary>
    Cancelled = 4
}
