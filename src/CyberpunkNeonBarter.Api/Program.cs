using CyberpunkNeonBarter.Api.Endpoints;
using CyberpunkNeonBarter.Infrastructure.Data;
using CyberpunkNeonBarter.Infrastructure.Repositories;
using CyberpunkNeonBarter.Infrastructure.Services.CycleSearch;
using CyberpunkNeonBarter.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL + EF Core
var connectionString = builder.Configuration.GetConnectionString("Barter")
    ?? throw new InvalidOperationException(
        "Не задано рядок підключення ConnectionStrings:Barter у appsettings.json");

builder.Services.AddDbContext<BarterDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(BarterDbContext).Assembly.FullName);
        // npgsql.EnableRetryOnFailure(maxRetryCount: 3); // ЗАКОМЕНТОВАНО, бо конфліктує з транзакціями
    }));

// OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Введіть ваш JWT токен"
        });

        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

// --- НАЛАШТУВАННЯ JWT АВТОРИЗАЦІЇ ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is missing in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
builder.Services.AddAuthorization();
// -------------------------------------

// Сервіси пошуку циклів обміну
builder.Services.AddScoped<GraphBuilder>();
builder.Services.AddScoped<CycleFinderService>();
builder.Services.AddScoped<CycleToDealService>();

// Реєстрація репозиторію та прикладної служби
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<OfferService>();

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

// --- ДОДАНО ДЛЯ ВІДОБРАЖЕННЯ HTML-СТОРІНКИ ---
app.UseDefaultFiles();
app.UseStaticFiles();
// ----------------------------------------------

app.UseHttpsRedirection();

// --- ПІДКЛЮЧЕННЯ MIDDLEWARE АВТОРИЗАЦІЇ ---
app.UseAuthentication();
app.UseAuthorization();
// ------------------------------------------

// Перевірка стану API та підключення до БД
app.MapGet("/health", async (BarterDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { status = "healthy", database = "postgres" })
        : Results.Problem("Database unavailable");
});

// Мапінг всіх ендпоінтів
app.MapAuthEndpoints();    
app.MapCycleEndpoints();   
app.MapDealEndpoints();    
app.MapOfferEndpoints();   

app.Run();