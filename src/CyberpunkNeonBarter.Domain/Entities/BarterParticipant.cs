namespace CyberpunkNeonBarter.Domain.Entities;

/// <summary>
/// Крок циклу: учасник угоди, його оголошення «віддаю» і оголошення «потребую», яке закривається в цій угоді.
/// Приклад кроку 1: A віддає GiveOffer (A) → B, і отримує NeedOffer (A) від C.
/// </summary>
public class BarterParticipant
{
    public Guid Id { get; set; }

    public Guid DealId { get; set; }
    public BarterDeal? Deal { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Оголошення «віддаю» цього учасника.</summary>
    public Guid GiveOfferId { get; set; }
    public Offer? GiveOffer { get; set; }

    /// <summary>Оголошення «потребую» цього учасника, яке задовольняється в угоді.</summary>
    public Guid NeedOfferId { get; set; }
    public Offer? NeedOffer { get; set; }

    /// <summary>Порядковий номер кроку в циклі: 1, 2 або 3.</summary>
    public int StepOrder { get; set; }

    /// <summary>Чи підтвердив учасник виконання своєї передачі.</summary>
    public bool TransferConfirmed { get; set; }
}
