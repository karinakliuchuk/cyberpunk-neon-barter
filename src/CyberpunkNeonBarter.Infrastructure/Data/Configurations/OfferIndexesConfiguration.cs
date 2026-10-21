using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberpunkNeonBarter.Infrastructure.Data.Configurations;

/// <summary>
/// Додаткові індекси для Offer, потрібні для NFR-02.
/// Не дублює індекс із Domain/Configurations/OfferConfiguration.cs.
/// </summary>
public sealed class OfferIndexesConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        // 1) Швидкий пошук власних активних оголошень користувача.
        //    Використовується для перевірки передумов UC-02 (FR-02).
        builder.HasIndex(o => new { o.UserId, o.Status, o.Type })
               .HasDatabaseName("ix_offers_user_status_type");

        // 2) Швидкий пошук активних Give за категорією (джерело ребер графа).
        builder.HasIndex(o => new { o.Status, o.Type, o.Category, o.Quantity })
               .HasDatabaseName("ix_offers_active_type_category_quantity");

        // 3) Швидкий пошук активних Need за категорією (ціль ребер графа).
        //    (індекс #2 вже покриває це через Type, але явний індекс допомагає планеру)
        builder.HasIndex(o => new { o.Status, o.Category })
               .HasDatabaseName("ix_offers_active_category");
    }
}