using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberpunkNeonBarter.Domain.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Quantity).IsRequired();

        // User 1 → 0..* Offer
        builder.HasOne(o => o.User)
               .WithMany(u => u.Offers)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Для швидкого пошуку активних оголошень певної категорії та типу (NFR-02)
        builder.HasIndex(o => new { o.Status, o.Category, o.Type });
    }
}
