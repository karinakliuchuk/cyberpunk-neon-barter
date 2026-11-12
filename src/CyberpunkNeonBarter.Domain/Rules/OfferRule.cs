namespace CyberpunkNeonBarter.Domain.Rules;

/// <summary>
/// Предметні правила оголошення, що не залежать від бази, сервера чи HTTP (спринт S2).
/// </summary>
public static class OfferRule
{
    /// <summary>
    /// Перевіряє термін придатності оголошення.
    /// Якщо термін не задано (null), оголошення вважається безстроковим і коректним.
    /// Інакше дата не повинна бути раніше за поточну календарну дату (UTC):
    /// сьогоднішній день допустимий, вчорашній — ні.
    /// </summary>
    /// <param name="date">Дата завершення дії оголошення або null.</param>
    /// <param name="now">Поточний момент; параметр потрібен для тестів, за замовчуванням <see cref="DateTime.UtcNow"/>.</param>
    public static bool IsValid(DateTime? date, DateTime? now = null)
    {
        if (date is null)
            return true;

        var today = (now ?? DateTime.UtcNow).Date;
        return date.Value.Date >= today;
    }
}
