using System.Net.Http.Json;
using ECommerce.Modules.Products;
using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.Host.Tests;

/// <summary>
/// One real end-to-end slice — HTTP request through FastEndpoints, the actual mediator, a real (SQLite)
/// DbContext, and back out as JSON — proving the wiring itself works, not just each handler in isolation.
/// Deliberately scoped to the Products module only (public, AllowAnonymous endpoint) rather than the
/// full Program.cs, which requires a reachable SQL Server/RabbitMQ to start.
/// </summary>
public sealed class ProductsEndpointIntegrationTests : IAsyncLifetime
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private SqliteConnection _connection = null!;
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddDbContext<ProductsDbContext>(o => o.UseSqlite(_connection));
        builder.Services.AddMediator(typeof(ProductsModule).Assembly);
        builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(ProductsModule).Assembly]);

        _app = builder.Build();
        _app.UseFastEndpoints();

        await _app.StartAsync();

        using (IServiceScope scope = _app.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ProductsDbContext>().Database.EnsureCreatedAsync();

        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        _connection.Dispose();
    }

    [Fact]
    public async Task GET_products_returns_a_seeded_product_through_the_full_http_to_db_pipeline()
    {
        using (IServiceScope scope = _app.Services.CreateScope())
        {
            ProductsDbContext db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
            Category category = Category.Create("Electronics", null, "system");
            db.Categories.Add(category);
            db.Products.Add(Product.Create("Widget", "A widget", "WID-001", 19.99m, category.Id, null, "system"));
            await db.SaveChangesAsync();
        }

        HttpResponseMessage response = await _client.GetAsync("/api/products?page=1&pageSize=20");
        response.EnsureSuccessStatusCode();

        PagedResult<ProductResponse>? body = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>(JsonOptions);

        Assert.NotNull(body);
        Assert.Single(body!.Items);
        Assert.Equal("Widget", body.Items[0].Name);
        Assert.Equal("WID-001", body.Items[0].SKU);
    }

    [Fact]
    public async Task GET_products_returns_an_empty_page_when_nothing_is_seeded()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/products?page=1&pageSize=20");
        response.EnsureSuccessStatusCode();

        PagedResult<ProductResponse>? body = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>(JsonOptions);

        Assert.NotNull(body);
        Assert.Empty(body!.Items);
        Assert.Equal(0, body.TotalCount);
    }
}
