using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberpunkNeonBarter.Domain.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Callsign).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Contact).IsRequired().HasMaxLength(200);

        // Позивний унікальний у межах системи
        builder.HasIndex(u => u.Callsign).IsUnique();
    }
}
