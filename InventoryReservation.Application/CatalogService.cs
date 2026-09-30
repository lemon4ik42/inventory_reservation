using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class CatalogService
{
    public CatalogService(IInventoryCatalog catalog, IInventoryUnitOfWork unitOfWork)
    {
        _catalog = catalog;
        _unitOfWork = unitOfWork;
    }

    private readonly IInventoryCatalog _catalog;
    private readonly IInventoryUnitOfWork _unitOfWork;

    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        product.EnsureActive();
        _catalog.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        _catalog.Add(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return _catalog.FindProductAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(
        int page,
        int pageSize,
        string? category,
        CancellationToken cancellationToken)
    {
        return _catalog.FindProductsAsync(page, pageSize, category, cancellationToken);
    }

    public Task<IReadOnlyList<StockItem>> GetStockAsync(
        Guid? warehouseId,
        Guid? productId,
        int? minAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return _catalog.FindStockAsync(warehouseId,
            productId,
            minAvailable,
            page,
            pageSize,
            cancellationToken);
    }

    public Task<IReadOnlyList<InventoryMovement>> GetMovementsAsync(int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return _catalog.FindMovementsAsync(page, pageSize, cancellationToken);
    }
}

