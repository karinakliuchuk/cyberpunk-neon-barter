using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;

// Знаходить замкнений цикл A→B→C→A з трьох різних користувачів (FR-06, AC-06).
public sealed class CycleFinderService(GraphBuilder graphBuilder, BarterDbContext db)
{
    public async Task<FoundCycle?> FindAsync(CancellationToken ct = default)
    {
        // 1. Побудувати граф (один SQL-запит)
        var graph = await graphBuilder.BuildAsync(ct);
        if (graph.EdgeCount == 0) return null;

        // 2. Знайти всі трійки A→B→C→A
        var candidates = EnumerateTriangles(graph);
        if (candidates.Count == 0) return null;

        // 3. Відфільтрувати валідні за NFR-03 (усі три користувачі різні, кількості збігаються)
        var valid = candidates.Where(c => IsValid(c, graph)).ToList();
        if (valid.Count == 0) return null;

        // 4. Детермінований вибір (A4, NFR-03, AC-11):
        var chosen = valid
            .OrderBy(c => new[] { c.A, c.B, c.C }.Min())
            .ThenBy(c => new[] { c.A, c.B, c.C }.OrderBy(x => x).ElementAt(1))
            .First();

        // 5. Перевірити, що всі три оголошення ще активні (A5, FR-08)
        var stillActive = await AreOffersStillActiveAsync(chosen, ct);
        if (!stillActive) return null;

        return new FoundCycle(
            chosen.A, chosen.B, chosen.C,
            chosen.GiveA.Id, chosen.GiveB.Id, chosen.GiveC.Id,
            chosen.NeedA.Id, chosen.NeedB.Id, chosen.NeedC.Id);
    }

    private static List<CycleCandidate> EnumerateTriangles(Graph graph)
    {
        var result = new List<CycleCandidate>();
        var seen = new HashSet<(Guid, Guid, Guid)>();

        foreach (var (a, edgesAB) in graph.Adjacency)
        {
            foreach (var edgeAB in edgesAB)
            {
                var b = edgeAB.ToUserId;

                if (!graph.Adjacency.TryGetValue(b, out var edgesBC)) continue;

                foreach (var edgeBC in edgesBC)
                {
                    var c = edgeBC.ToUserId;
                    if (c == a || c == b) continue;

                    if (!graph.Adjacency.TryGetValue(c, out var edgesCA)) continue;

                    var edgeCA = edgesCA.FirstOrDefault(e => e.ToUserId == a);
                    if (edgeCA is null) continue;

                    var canonical = CanonicalizeKeys(a, b, c);
                    if (!seen.Add(canonical)) continue;

                    result.Add(new CycleCandidate(
                        a, b, c,
                        edgeAB.GiveOffer, graph.NeedsByUser[a],
                        edgeBC.GiveOffer, graph.NeedsByUser[b],
                        edgeCA.GiveOffer, graph.NeedsByUser[c]));
                }
            }
        }

        return result;
    }

    private static bool IsValid(CycleCandidate c, Graph graph)
    {
        // три різні користувачі
        if (c.A == c.B || c.B == c.C || c.A == c.C) return false;

        // Give_A покриває Need_B
        if (c.GiveA.Category != c.NeedB.Category) return false;
        if (c.GiveA.Quantity != c.NeedB.Quantity) return false;

        // Give_B покриває Need_C
        if (c.GiveB.Category != c.NeedC.Category) return false;
        if (c.GiveB.Quantity != c.NeedC.Quantity) return false;

        // Give_C покриває Need_A
        if (c.GiveC.Category != c.NeedA.Category) return false;
        if (c.GiveC.Quantity != c.NeedA.Quantity) return false;

        return true;
    }

    private static (Guid, Guid, Guid) CanonicalizeKeys(Guid a, Guid b, Guid c)
    {
        Span<Guid> arr = stackalloc Guid[3] { a, b, c };
        arr.Sort();
        return (arr[0], arr[1], arr[2]);
    }

    private async Task<bool> AreOffersStillActiveAsync(CycleCandidate c, CancellationToken ct)
    {
        var ids = new[] { c.GiveA.Id, c.GiveB.Id, c.GiveC.Id };
        var count = await db.Offers
            .AsNoTracking()
            .CountAsync(o => ids.Contains(o.Id) && o.Status == Domain.Enums.OfferStatus.Active, ct);
        return count == 3;
    }
}