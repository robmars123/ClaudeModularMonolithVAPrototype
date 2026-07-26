using System.Text;
using System.Threading.RateLimiting;
using ECommerce.Host.Infrastructure;
using ECommerce.Host.Infrastructure.HealthChecks;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Inventory;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Products;
using ECommerce.Modules.Users;
using ECommerce.Shared.Behaviors;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
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

// Bound + validated JWT config — a missing/malformed value fails fast at startup (ValidateOnStart)
// instead of surfacing on the first login/token issuance.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

// JWT bearer auth. UsersModule's AddIdentity() also calls AddAuthentication() internally
// and explicitly sets Identity's own cookie scheme as the default authenticate/challenge
// scheme — the shorthand AddAuthentication(string) overload only sets the *fallback*
// DefaultScheme, which does not win over an already-set DefaultChallengeScheme, so the
// authenticate/challenge schemes must be set explicitly here to actually take over.
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
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// Cross-cutting mediator pipeline (outermost first): logs every command/query with duration,
// then validates requests implementing IValidatableRequest before they reach the handler.
builder.Services.AddPipelineBehavior(typeof(RequestLoggingBehavior<,>));
builder.Services.AddPipelineBehavior(typeof(ValidationBehavior<,>));

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

// CORS — restricted to the local client dev server (configurable for other environments) and to the
// verbs/headers the client actually sends; no cookies are used so credentials stay disallowed.
// An empty/missing list in Development falls back to the local Vite dev server; outside Development
// it's a misconfiguration (would otherwise silently block every legitimate cross-origin request), so
// it fails fast at startup — same treatment as the JWT secret check above.
var clientOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (clientOrigins.Length == 0)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Cors:AllowedOrigins must list at least one allowed origin outside Development.");

    clientOrigins = ["http://localhost:5173"];
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.WithOrigins(clientOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization", "X-Correlation-Id")
            .WithExposedHeaders("X-Correlation-Id"));
});

// X-Forwarded-For/-Proto are only honored when they arrive via a known/trusted proxy (the default
// ASP.NET Core behavior, restricted to loopback). docker-compose.yml exposes the API port directly
// with no reverse proxy in front of it today, so RemoteIpAddress below is already the real client IP;
// clearing KnownNetworks/KnownProxies here would let ANY caller spoof X-Forwarded-For and get a fresh
// rate-limit partition on every request, bypassing the auth rate limiter entirely. If a real reverse
// proxy is added in front of the API later, add its address to KnownProxies (or its subnet to
// KnownNetworks) instead of clearing them.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Rate limiting: a generous global per-IP ceiling, tightened further for the auth endpoints
// (login/register/refresh) where brute force and credential stuffing are the actual risk. Partitioned
// by IP + path-class so the two limits are enforced independently per client. Health checks are
// exempt — they're polled frequently by infra and must never flip "unhealthy" from a 429.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (context.Request.Path.StartsWithSegments("/health"))
            return RateLimitPartition.GetNoLimiter("health");

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isAuthPath = context.Request.Path.StartsWithSegments("/api/auth");

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: isAuthPath ? $"auth:{ip}" : $"general:{ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isAuthPath ? 10 : 200,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

var app = builder.Build();

// Migrate and seed the database on startup
await DatabaseMigrator.MigrateAndSeedAsync(app);

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("Client");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen(); // Swagger UI served at "/swagger"

// Liveness: process is up, no dependency checks. Readiness: DB + broker must both respond.
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.Run();
