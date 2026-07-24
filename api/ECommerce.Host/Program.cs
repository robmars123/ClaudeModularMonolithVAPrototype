using System.Text;
using ECommerce.Host.Infrastructure;
using ECommerce.Host.Infrastructure.HealthChecks;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Inventory;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Products;
using ECommerce.Modules.Users;
using ECommerce.Shared.Behaviors;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string PlaceholderJwtSecret = "ECommerce-SuperSecretKey-ChangeInProduction-MinLength32Chars!";
if (!builder.Environment.IsDevelopment() &&
    builder.Configuration["JwtSettings:Secret"] == PlaceholderJwtSecret)
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is still the checked-in development placeholder. " +
        "Set a real secret via environment variable/user-secrets before running outside Development.");
}

// Register all modules
new UsersModule().RegisterModule(builder.Services, builder.Configuration);
new ProductsModule().RegisterModule(builder.Services, builder.Configuration);
new CustomersModule().RegisterModule(builder.Services, builder.Configuration);
new InventoryModule().RegisterModule(builder.Services, builder.Configuration);
new OrdersModule().RegisterModule(builder.Services, builder.Configuration);

// JWT bearer auth. UsersModule's AddIdentity() also calls AddAuthentication() internally
// and explicitly sets Identity's own cookie scheme as the default authenticate/challenge
// scheme — the shorthand AddAuthentication(string) overload only sets the *fallback*
// DefaultScheme, which does not win over an already-set DefaultChallengeScheme, so the
// authenticate/challenge schemes must be set explicitly here to actually take over.
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// Cross-cutting mediator pipeline: logs every command/query with duration.
builder.Services.AddPipelineBehavior(typeof(RequestLoggingBehavior<,>));

// Correlation id flows HTTP request -> mediator handler -> outbox row -> RabbitMQ message -> consumer.
builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

// Messaging: RabbitMQ publisher/dispatcher always registered (tolerant of the broker being down).
// Each module registers its own IIntegrationEventPublisher/IOutboxStore against its own DbContext.
builder.Services.AddRabbitMqMessaging(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHealthChecks()
    .AddCheck("database", new SqlDatabaseHealthCheck(connectionString), tags: ["ready"])
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"]);

// FastEndpoints — discovers Endpoint classes across all module assemblies
builder.Services.AddFastEndpoints(o => o.Assemblies =
[
    typeof(UsersModule).Assembly,
    typeof(ProductsModule).Assembly,
    typeof(CustomersModule).Assembly,
    typeof(InventoryModule).Assembly,
    typeof(OrdersModule).Assembly
]);

// Swagger (FastEndpoints.Swagger — NSwag-based, understands FastEndpoints' endpoint model)
builder.Services.SwaggerDocument(o =>
{
    o.EnableJWTBearerAuth = true;
    o.DocumentSettings = s =>
    {
        s.Title = "ECommerce Modular Monolith API";
        s.Version = "v1";
        s.Description = "Production-ready modular monolith e-commerce API.";
    };
});

// CORS — restricted to the local client dev server (configurable for other environments).
var clientOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.WithOrigins(clientOrigins).AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Migrate and seed the database on startup
await DatabaseMigrator.MigrateAndSeedAsync(app);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen(); // Swagger UI served at "/swagger"

// Liveness: process is up, no dependency checks. Readiness: DB + broker must both respond.
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.Run();
