using CyberpunkNeonBarter.Domain.Enums;

namespace CyberpunkNeonBarter.Api.DTOs;

/// <summary>
/// Параметри пошуку оголошень (спринт S1). Усі поля необов'язкові:
/// незаповнене поле означає «без обмеження за цією ознакою».
/// </summary>
/// <param name="Category">Категорія ресурсу (вода, їжа, ліки, зв'язок, енергія).</param>
/// <param name="Type">Тип оголошення: «віддаю» або «потребую».</param>
/// <param name="Status">Статус оголошення; за замовчуванням шукаємо лише активні.</param>
public sealed record OfferFilterDto(
    ResourceCategory? Category = null,
    OfferType? Type = null,
    OfferStatus? Status = OfferStatus.Active);
