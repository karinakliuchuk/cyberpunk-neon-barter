using CyberpunkNeonBarter.Api.Endpoints;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL + EF Core
var connectionString = builder.Configuration.GetConnectionString("Barter")
    ?? throw new InvalidOperationException(
        "Не задано рядок підключення ConnectionStrings:Barter у appsettings.json");

builder.Services.AddDbContext<BarterDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(BarterDbContext).Assembly.FullName);
        npgsql.EnableRetryOnFailure(maxRetryCount: 3);
    }));

// OpenAPI
builder.Services.AddOpenApi();

// Сервіси пошуку циклів обміну
builder.Services.AddScoped<GraphBuilder>();
builder.Services.AddScoped<CycleFinderService>();
builder.Services.AddScoped<CycleToDealService>();

var app = builder.Build();

// OpenAPI у режимі розробки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Cyberpunk Neon Barter API v1");
    });
}

app.UseHttpsRedirection();

// Перевірка стану API та підключення до БД
app.MapGet("/health", async (BarterDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { status = "healthy", database = "postgres" })
        : Results.Problem("Database unavailable");
});

// Ендпоїнти пошуку циклів обміну
app.MapCycleEndpoints();

app.MapDealEndpoints();

app.MapOfferEndpoints();


app.Run();