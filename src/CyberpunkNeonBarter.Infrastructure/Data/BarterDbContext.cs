using CyberpunkNeonBarter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Infrastructure.Data;

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
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CyberpunkNeonBarter.Domain.Entities.User).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BarterDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureAdditionalConstraints(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Offer>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_offers_quantity_positive",
                "\"Quantity\" > 0"));

        modelBuilder.Entity<BarterParticipant>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_barter_participants_step_order",
                "\"StepOrder\" >= 1 AND \"StepOrder\" <= 3"));
    }
}