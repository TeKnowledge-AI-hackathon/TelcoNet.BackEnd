using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Scalar.AspNetCore;
using TelcoNet.API.Middleware;
using TelcoNet.Core.Interfaces;
using TelcoNet.Core.Services;
using TelcoNet.Data;
using TelcoNet.Data.Seed;
using TelcoNet.Plugins;

// ── Load environment variables ──
// Try solution root first, then current directory
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
if (File.Exists(envPath))
    DotNetEnv.Env.Load(envPath);
else
    DotNetEnv.Env.Load(); // fallback to current directory

var builder = WebApplication.CreateBuilder(args);

// ── Configuration from .env ──
var azureEndpoint = (Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "").Trim().TrimEnd('\\');
if (string.IsNullOrEmpty(azureEndpoint)) throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set");

var azureApiKey = (Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "").Trim().TrimEnd('\\');
if (string.IsNullOrEmpty(azureApiKey)) throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set");

var azureModelId = (Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL_ID") ?? "").Trim().TrimEnd('\\');
if (string.IsNullOrEmpty(azureModelId)) throw new InvalidOperationException("AZURE_OPENAI_MODEL_ID is not set");

var jwtSecret = (Environment.GetEnvironmentVariable("JWT_SECRET") ?? "TelcoNet-Hackathon-SuperSecret-Key-2026!!").Trim().TrimEnd('\\');
var jwtIssuer = (Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "TelcoNet.API").Trim().TrimEnd('\\');
var jwtAudience = (Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "TelcoNet.Client").Trim().TrimEnd('\\');

// ── Database (SQLite) ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=telconet.db"));

// ── JWT Authentication ──
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── Core Services ──
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICopilotService, CopilotService>();

// ── Semantic Kernel + Plugins ──
builder.Services.AddScoped<Kernel>(sp =>
{
    var networkService = sp.GetRequiredService<INetworkService>();
    var dashboardService = sp.GetRequiredService<IDashboardService>();

    var kernelBuilder = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: azureModelId,
            endpoint: azureEndpoint,
            apiKey: azureApiKey
        );

    // Register all plugins with their dependencies
    kernelBuilder.Plugins.AddFromObject(new NetworkQueryPlugin(networkService), "NetworkQuery");
    kernelBuilder.Plugins.AddFromObject(new OutageDetectionPlugin(networkService), "OutageDetection");
    kernelBuilder.Plugins.AddFromObject(new CoverageFinderPlugin(networkService), "CoverageFinder");
    kernelBuilder.Plugins.AddFromObject(new KpiPlugin(dashboardService), "KPI");
    kernelBuilder.Plugins.AddFromObject(new AlertPlugin(networkService), "AlertMonitor");

    return kernelBuilder.Build();
});

// ── API Services ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "v1";
    config.Title = "TelcoNet API";
    config.Version = "v1";

    config.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token here. You do NOT need to type 'Bearer '."
    });

    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

// ── CORS (so frontend can call the API) ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Middleware Pipeline ──
app.UseMiddleware<ExceptionMiddleware>();

// API Documentation (Swagger UI using NSwag)
app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents
app.UseSwaggerUi(); // Serves the Swagger UI web ui

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLoggingMiddleware>();
app.MapControllers();

// ── Database Migration + Seed Data ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║           TelcoNet API — Running!                  ║");
Console.WriteLine("║  Swagger Docs: http://localhost:5153/swagger        ║");
Console.WriteLine("║  Azure OpenAI: Connected                           ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");

app.Run();
