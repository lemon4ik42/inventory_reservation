using InventoryReservation.Application;
using InventoryReservation.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = builder.Configuration.GetConnectionString("Inventory")
            ?? throw new InvalidOperationException("Inventory database connection string is missing.");
        builder.Services.AddInfrastructure(databaseProvider, connectionString);
        builder.Services.AddScoped<HighestAvailableStockStrategy>();
        builder.Services.AddScoped<HighestPriorityWarehouseStrategy>();
        builder.Services.AddScoped<IWarehouseSelectionStrategy>(serviceProvider =>
            builder.Configuration["Reservation:Strategy"] == "HighestPriority"
                ? serviceProvider.GetRequiredService<HighestPriorityWarehouseStrategy>()
                : serviceProvider.GetRequiredService<HighestAvailableStockStrategy>());
        builder.Services.AddScoped<IInventoryOperationFactory, InboundOperationFactory>();
        builder.Services.AddScoped<IInventoryOperationFactory, OutboundOperationFactory>();
        builder.Services.AddScoped<InventoryOperationExecutor>();
        builder.Services.AddScoped<InventoryService>();
        builder.Services.AddScoped<CatalogService>();
        var app = builder.Build();
        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                dbContext.Database.EnsureCreated();
            }
            else
            {
                dbContext.Database.Migrate();
            }
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}

