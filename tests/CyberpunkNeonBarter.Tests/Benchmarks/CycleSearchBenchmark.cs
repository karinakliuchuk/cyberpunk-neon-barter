using BenchmarkDotNet.Attributes;
using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Tests.Benchmarks;

[MemoryDiagnoser]
public class CycleSearchBenchmark
{
    private BarterDbContext _db = null!;

    [GlobalSetup]
    public void Setup()
    {
        var opts = new DbContextOptionsBuilder<BarterDbContext>()
            .UseInMemoryDatabase("bench-" + Guid.NewGuid())
            .Options;
        _db = new BarterDbContext(opts);

        // 1000 оголошень: 500 Give + 500 Need
        var users = Enumerable.Range(0, 500).Select(_ => Guid.NewGuid()).ToList();
        var rnd = new Random(42);
        var categories = Enum.GetValues<ResourceCategory>();

        foreach (var u in users)
        {
            _db.Offers.Add(Offer.Create(u, OfferType.Give, categories[rnd.Next(categories.Length)], rnd.Next(1, 20)));
            _db.Offers.Add(Offer.Create(u, OfferType.Need, categories[rnd.Next(categories.Length)], rnd.Next(1, 20)));
        }
        _db.SaveChanges();
    }

    [Benchmark]
    public async Task<FoundCycle?> Find_1000_Offers()
    {
        var finder = new CycleFinderService(new GraphBuilder(_db), _db);
        return await finder.FindAsync();
    }
}