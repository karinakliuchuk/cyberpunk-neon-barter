using CyberpunkNeonBarter.Domain.Entities;
using CyberpunkNeonBarter.Domain.Enums;
using CyberpunkNeonBarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;

// Будує граф відповідностей "хто кому може віддати".
public sealed class GraphBuilder(BarterDbContext db)
{
    // Завантажує активні оголошення з БД і будує граф.
    public async Task<Graph> BuildAsync(CancellationToken ct = default)
    {
        var activeOffers = await db.Offers
            .AsNoTracking()
            .Where(o => o.Status == OfferStatus.Active)
            .ToListAsync(ct);

        var gives = activeOffers
            .Where(o => o.Type == OfferType.Give)
            .GroupBy(o => o.UserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAtUtc).First());

        var needs = activeOffers
            .Where(o => o.Type == OfferType.Need)
            .GroupBy(o => o.UserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAtUtc).First());

        var adjacency = new Dictionary<Guid, List<Edge>>();
        var edges = new List<Edge>();

        // Gives × Needs — O(G*N). Для 1000 оголошень ~ 62 500 ітерацій.
        foreach (var (giverId, give) in gives)
        {
            foreach (var (receiverId, need) in needs)
            {
                if (giverId == receiverId) continue;
                if (give.Category != need.Category) continue;
                if (give.Quantity != need.Quantity) continue;

                var edge = new Edge(giverId, receiverId, give, need);
                edges.Add(edge);

                if (!adjacency.TryGetValue(giverId, out var list))
                {
                    list = new List<Edge>();
                    adjacency[giverId] = list;
                }
                list.Add(edge);
            }
        }

        return new Graph(adjacency, edges, gives, needs);
    }
}

public sealed class Graph
{
    public IReadOnlyDictionary<Guid, List<Edge>> Adjacency { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyDictionary<Guid, Offer> GivesByUser { get; }
    public IReadOnlyDictionary<Guid, Offer> NeedsByUser { get; }

    internal Graph(
        Dictionary<Guid, List<Edge>> adjacency,
        List<Edge> edges,
        Dictionary<Guid, Offer> gives,
        Dictionary<Guid, Offer> needs)
    {
        Adjacency = adjacency;
        Edges = edges;
        GivesByUser = gives;
        NeedsByUser = needs;
    }

    public int NodeCount => GivesByUser.Count;
    public int EdgeCount => Edges.Count;
}