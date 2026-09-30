using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace InventoryReservation.Api.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<TestApiFactory>
{
    public ApiSmokeTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private readonly HttpClient _client;

    [Fact]
    public async Task Root_returns_not_found()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_endpoint_returns_not_found()
    {
        var response = await _client.GetAsync("/api/no-such-resource");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Product_can_be_created_through_api()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new { sku = "TEST-001", name = "Test product", category = "Tests", supplierId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

