using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberpunkNeonBarter.Domain.Configurations;

public class BarterDealConfiguration : IEntityTypeConfiguration<BarterDeal>
{
    public void Configure(EntityTypeBuilder<BarterDeal> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        // BarterDeal 1 → 3 BarterParticipant (кількість «рівно 3» гарантує логіка створення угоди)
        builder.HasMany(d => d.Participants)
               .WithOne(p => p.Deal)
               .HasForeignKey(p => p.DealId)
               .OnDelete(DeleteBehavior.Cascade);

        // Допоміжна властивість не зберігається в базі
        builder.Ignore(d => d.HasValidCycleSize);
    }
}
