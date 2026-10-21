using CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;

namespace CyberpunkNeonBarter.Api.Endpoints;

public static class CycleEndpoints
{
    public static IEndpointRouteBuilder MapCycleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cycles/find", async (
            CycleFinderService finder,
            CycleToDealService dealService,
            CancellationToken ct) =>
        {
            var cycle = await finder.FindAsync(ct);
            if (cycle is null)
                return Results.Conflict(new
                {
                    code = "cycle_not_found",
                    message = "Обмін не знайдено"
                });

            var deal = await dealService.CreateDealAsync(cycle, ct);

            return Results.Ok(new
            {
                dealId = deal.Id,
                status = deal.Status.ToString(),
                steps = new[]
                {
                    new { order = 1, from = cycle.ParticipantA, to = cycle.ParticipantB, giveOfferId = cycle.GiveOfferA },
                    new { order = 2, from = cycle.ParticipantB, to = cycle.ParticipantC, giveOfferId = cycle.GiveOfferB },
                    new { order = 3, from = cycle.ParticipantC, to = cycle.ParticipantA, giveOfferId = cycle.GiveOfferC }
                }
            });
        })
        .WithName("FindCycle")
        .WithOpenApi();

        return app;
    }
}