namespace CyberpunkNeonBarter.Api.DTOs;

public class DealResponse
{
    public Guid DealId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// Для примусового скасування адміном (UC-07)
public class AdminCancelDealRequest
{
    public Guid DealId { get; set; }
    public string Reason { get; set; } = string.Empty;
}