using InventoryReservation.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Infrastructure;

public sealed class EfStockRepository : IStockRepository
{
    public EfStockRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private readonly InventoryDbContext _dbContext;

    public Task<Warehouse?> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        return _dbContext.Warehouses.Include(warehouse => warehouse.StockItems)
            .SingleOrDefaultAsync(warehouse => warehouse.Id == warehouseId, cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return _dbContext.Products.SingleOrDefaultAsync(product => product.Id == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<StockItem>> GetAvailableForProductAsync(Guid productId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.StockItems.Include(item => item.Product).Include(item => item.Warehouse)
            .Where(item => item.ProductId == productId && item.Product.IsActive &&
                item.OnHandQuantity > item.ReservedQuantity)
            .ToListAsync(cancellationToken);
    }
}

