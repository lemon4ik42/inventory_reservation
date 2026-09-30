using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InventoryReservation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryReservation.Api.IntegrationTests;

public sealed class InventoryWorkflowTests
{
    [Theory]
    [InlineData("cancel", 5)]
    [InlineData("ship", 0)]
    public async Task Fully_reserved_order_can_be_completed(string operation, int expectedStock)
    {
        using var factory = new SqliteApiFactory();
        using var client = factory.CreateClient();
        factory.Initialize();
        const string productId = "55555555-5555-5555-5555-555555555555";
        const string warehouseId = "44444444-4444-4444-4444-444444444444";

        var receipt = await client.PostAsync(
            $"/api/stock/receipts?warehouseId={warehouseId}&productId={productId}&quantity=5", null);
        Assert.Equal(HttpStatusCode.NoContent, receipt.StatusCode);

        var created = await client.PostAsync(
            "/api/orders?customerId=22222222-2222-2222-2222-222222222222", null);
        created.EnsureSuccessStatusCode();
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = json.GetProperty("id").GetGuid();

        await AssertSuccessAsync(client, $"/api/orders/{orderId}/items?productId={productId}&quantity=5");
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/confirm");
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/reserve");
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/{operation}");

        var stock = await client.GetFromJsonAsync<JsonElement>("/api/stock");
        Assert.Equal(expectedStock, stock[0].GetProperty("onHandQuantity").GetInt32());
        Assert.Equal(0, stock[0].GetProperty("reservedQuantity").GetInt32());

        var repeated = await client.PostAsync($"/api/orders/{orderId}/{operation}", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, repeated.StatusCode);
        var movements = await client.GetFromJsonAsync<JsonElement>("/api/inventory-movements");
        Assert.Equal(3, movements.GetArrayLength());
    }

    [Fact]
    public async Task Concurrent_contexts_cannot_reserve_same_last_item()
    {
        using var factory = new SqliteApiFactory();
        using var client = factory.CreateClient();
        factory.Initialize();
        await AssertSuccessAsync(client,
            "/api/stock/receipts?warehouseId=44444444-4444-4444-4444-444444444444" +
            "&productId=55555555-5555-5555-5555-555555555555&quantity=1");

        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var second = secondScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var firstStock = await first.StockItems.Include(item => item.Product).SingleAsync();
        var secondStock = await second.StockItems.Include(item => item.Product).SingleAsync();

        firstStock.Reserve(1);
        secondStock.Reserve(1);
        await first.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Failed_reservation_does_not_persist_partial_changes()
    {
        using var factory = new SqliteApiFactory();
        using var client = factory.CreateClient();
        factory.Initialize();
        const string productId = "55555555-5555-5555-5555-555555555555";
        await AssertSuccessAsync(client,
            "/api/stock/receipts?warehouseId=44444444-4444-4444-4444-444444444444" +
            $"&productId={productId}&quantity=1");

        var productResponse = await client.PostAsJsonAsync("/api/products", new
        {
            sku = "SECOND",
            name = "Unavailable product",
            category = "Tests",
            supplierId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });
        productResponse.EnsureSuccessStatusCode();
        var productJson = await productResponse.Content.ReadFromJsonAsync<JsonElement>();
        var secondProductId = productJson.GetProperty("id").GetGuid();

        var created = await client.PostAsync(
            "/api/orders?customerId=22222222-2222-2222-2222-222222222222", null);
        created.EnsureSuccessStatusCode();
        var orderJson = await created.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = orderJson.GetProperty("id").GetGuid();
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/items?productId={productId}&quantity=1");
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/items?productId={secondProductId}&quantity=1");
        await AssertSuccessAsync(client, $"/api/orders/{orderId}/confirm");

        var failed = await client.PostAsync($"/api/orders/{orderId}/reserve", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, failed.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var stock = await dbContext.StockItems.SingleAsync();
        Assert.Equal(0, stock.ReservedQuantity);
        Assert.Empty(await dbContext.StockReservations.ToListAsync());
        Assert.Single(await dbContext.InventoryMovements.ToListAsync());
        var order = await dbContext.SalesOrders.SingleAsync();
        Assert.Equal(InventoryReservation.Domain.OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task Out_of_range_page_returns_validation_error()
    {
        using var factory = new SqliteApiFactory();
        using var client = factory.CreateClient();
        factory.Initialize();

        var response = await client.GetAsync("/api/products?page=2147483647&pageSize=100");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
    private static async Task AssertSuccessAsync(HttpClient client, string path)
    {
        var response = await client.PostAsync(path, null);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }
}

