using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberpunkNeonBarter.Domain.Configurations;

public class BarterParticipantConfiguration : IEntityTypeConfiguration<BarterParticipant>
{
    public void Configure(EntityTypeBuilder<BarterParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        // User 1 → 0..* BarterParticipant
        builder.HasOne(p => p.User)
               .WithMany(u => u.Participations)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Два зв'язки з Offer: оголошення «віддаю» і «потребую»
        builder.HasOne(p => p.GiveOffer)
               .WithMany()
               .HasForeignKey(p => p.GiveOfferId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.NeedOffer)
               .WithMany()
               .HasForeignKey(p => p.NeedOfferId)
               .OnDelete(DeleteBehavior.Restrict);

        // У межах однієї угоди номер кроку та учасник не повторюються
        builder.HasIndex(p => new { p.DealId, p.StepOrder }).IsUnique();
        builder.HasIndex(p => new { p.DealId, p.UserId }).IsUnique();
    }
}
