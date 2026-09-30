using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryReservation.Infrastructure;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Host=localhost;Database=inventory_reservation;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseNpgsql(connectionString)
            .Options;
        return new InventoryDbContext(options);
    }
}

