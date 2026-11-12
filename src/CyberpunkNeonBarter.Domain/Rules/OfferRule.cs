namespace CyberpunkNeonBarter.Domain.Rules;

public static class OfferRule
{
    public static bool IsValid(DateTime? expiresAt)
    {
        // Якщо дата вказана і вона вже минула — правило не пройдено
        if (expiresAt.HasValue && expiresAt.Value < DateTime.UtcNow)
            return false;

        // Якщо дата в майбутньому або взагалі не вказана (null) — все ок
        return true;
    }
}