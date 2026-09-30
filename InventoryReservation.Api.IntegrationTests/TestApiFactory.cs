using InventoryReservation.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InventoryReservation.Api.IntegrationTests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(ConfigureTestServices);
    }

    private static void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<InventoryDbContext>>();
        services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase("inventory-api-tests"));
    }
}

