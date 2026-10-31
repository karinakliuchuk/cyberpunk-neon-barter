using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace CyberpunkNeonBarter.Tests.Services;

public sealed class CycleFinderServiceTests
{
    private readonly ITestOutputHelper _output;

    public CycleFinderServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static BarterDbContext NewDb(string name)
    {
        var opts = new DbContextOptionsBuilder<BarterDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new BarterDbContext(opts);
    }

    [Fact]
    public async Task Finds_Valid_Triangle()
    {
        await using var db = NewDb(nameof(Finds_Valid_Triangle));

        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        _output.WriteLine($"A={a}, B={b}, C={c}");

        db.Users.AddRange(
            new User { Id = a, Callsign = "A", Contact = "a@x" },
            new User { Id = b, Callsign = "B", Contact = "b@x" },
            new User { Id = c, Callsign = "C", Contact = "c@x" });

        db.Offers.AddRange(
            Offer.Create(a, OfferType.Give, ResourceCategory.Water, 10),
            Offer.Create(a, OfferType.Need, ResourceCategory.Energy, 5),
            Offer.Create(b, OfferType.Give, ResourceCategory.Energy, 5),
            Offer.Create(b, OfferType.Need, ResourceCategory.Food, 7),
            Offer.Create(c, OfferType.Give, ResourceCategory.Food, 7),
            Offer.Create(c, OfferType.Need, ResourceCategory.Water, 10));
        await db.SaveChangesAsync();

        var allOffers = await db.Offers.AsNoTracking().ToListAsync();
        _output.WriteLine($"Всього offers: {allOffers.Count}");
        foreach (var o in allOffers)
            _output.WriteLine($"  {o.Type} {o.Category} qty={o.Quantity} status={o.Status} user={o.UserId}");

        var builder = new GraphBuilder(db);
        var graph = await builder.BuildAsync();
        _output.WriteLine($"Graph: nodes={graph.NodeCount}, edges={graph.EdgeCount}");
        foreach (var e in graph.Edges)
            _output.WriteLine($"  Edge {e.FromUserId} → {e.ToUserId} " +
                              $"(Give {e.GiveOffer.Category}/{e.GiveOffer.Quantity} " +
                              $"→ Need {e.NeedOffer.Category}/{e.NeedOffer.Quantity})");

        var finder = new CycleFinderService(builder, db);
        var cycle = await finder.FindAsync();

        Assert.NotNull(cycle);
        Assert.Equal(3, new[] { cycle!.ParticipantA, cycle.ParticipantB, cycle.ParticipantC }.Distinct().Count());
    }

    [Fact]
    public async Task Returns_Null_When_No_Cycle()
    {
        await using var db = NewDb(nameof(Returns_Null_When_No_Cycle));

        var a = Guid.NewGuid();
        db.Users.Add(new User { Id = a, Callsign = "A", Contact = "a@x" });
        db.Offers.AddRange(
            Offer.Create(a, OfferType.Give, ResourceCategory.Water, 10),
            Offer.Create(a, OfferType.Need, ResourceCategory.Energy, 5));
        await db.SaveChangesAsync();

        var finder = new CycleFinderService(new GraphBuilder(db), db);
        var cycle = await finder.FindAsync();

        Assert.Null(cycle);
    }
}