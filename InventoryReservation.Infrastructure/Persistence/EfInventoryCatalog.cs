using InventoryReservation.Application;
using InventoryReservation.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Infrastructure;

public sealed class EfInventoryCatalog : IInventoryCatalog
{
    public EfInventoryCatalog(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private readonly InventoryDbContext _dbContext;

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }

    public void Add(Warehouse warehouse)
    {
        _dbContext.Warehouses.Add(warehouse);
    }

    public Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Products.AsNoTracking().SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> FindProductsAsync(
        int page,
        int pageSize,
        string? category,
        CancellationToken cancellationToken)
    {
        return await Paginate(
            _dbContext.Products.AsNoTracking().Where(product => category == null || product.Category == category)
            .OrderBy(product => product.Name),
            page,
            pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockItem>> FindStockAsync(
        Guid? warehouseId,
        Guid? productId,
        int? minAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.StockItems.AsNoTracking().Include(item => item.Product).Include(item => item.Warehouse)
            .Where(item => warehouseId == null || item.WarehouseId == warehouseId)
            .Where(item => productId == null || item.ProductId == productId)
            .Where(item => minAvailable == null || item.OnHandQuantity - item.ReservedQuantity >= minAvailable)
            .OrderBy(item => item.WarehouseId)
            .ThenBy(item => item.ProductId);
        return await Paginate(query, page, pageSize).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryMovement>> FindMovementsAsync(int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await Paginate(
            _dbContext.InventoryMovements.AsNoTracking().OrderByDescending(movement => movement.OccurredAtUtc),
            page,
            pageSize)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<T> Paginate<T>(IQueryable<T> query, int page, int pageSize)
    {
        var size = Math.Clamp(pageSize, 1, 100);
        var offset = (long)(Math.Max(page, 1) - 1) * size;

        if (offset > int.MaxValue)
        {
            throw new DomainRuleViolationException("Requested page exceeds supported range.");
        }

        return query.Skip((int)offset).Take(size);
    }
}

