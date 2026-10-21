using CyberpunkNeonBarter.Domain.Enums;

namespace CyberpunkNeonBarter.Api.DTOs;

public class CreateOfferRequest
{
    public Guid UserId { get; set; }
    public OfferType Type { get; set; }
    public ResourceCategory Category { get; set; }
    public int Quantity { get; set; }
}

public class OfferResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}