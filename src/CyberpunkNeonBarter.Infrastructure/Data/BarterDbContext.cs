using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Infrastructure.Data;

/// <summary>
/// Контекст доступу до PostgreSQL.
/// Конфігурації сутностей живуть у Domain/Configurations — підхоплюються автоматично.
/// Міграції — тут, у Infrastructure.
/// </summary>
public sealed class BarterDbContext(DbContextOptions<BarterDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<BarterDeal> BarterDeals => Set<BarterDeal>();
    public DbSet<BarterParticipant> BarterParticipants => Set<BarterParticipant>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфігурації з Domain (від колеги)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CyberpunkNeonBarter.Domain.Entities.User).Assembly);

        // Конфігурації з Infrastructure (ваші: індекси, CHECK-и)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BarterDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Обмеження рівня БД, які стосуються пошуку циклів (FR-04, FR-06).
    /// Не дублюємо те, що вже є в Domain/Configurations.
    /// </summary>
    private static void ConfigureAdditionalConstraints(ModelBuilder modelBuilder)
    {
        // Offer.Quantity > 0 (FR-04) — додаємо, бо в OfferConfiguration його немає
        modelBuilder.Entity<Offer>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_offers_quantity_positive",
                "\"Quantity\" > 0"));

        // BarterParticipant.StepOrder ∈ [1..3] — 3-сторонній цикл у v1.0
        modelBuilder.Entity<BarterParticipant>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_barter_participants_step_order",
                "\"StepOrder\" >= 1 AND \"StepOrder\" <= 3"));
    }
}