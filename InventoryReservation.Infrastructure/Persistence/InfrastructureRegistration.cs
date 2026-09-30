using InventoryReservation.Application;
using InventoryReservation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryReservation.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string provider,
        string connectionString)
    {
        services.AddDbContext<InventoryDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });
        services.AddScoped<ISalesOrderRepository, EfSalesOrderRepository>();
        services.AddScoped<IStockRepository, EfStockRepository>();
        services.AddScoped<IInventoryCatalog, EfInventoryCatalog>();
        services.AddScoped<IInventoryUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<InventoryDbContext>());
        return services;
    }
}

