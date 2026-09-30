using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public interface IInventoryCatalog
{
    void Add(Product product);

    void Add(Warehouse warehouse);

    Task<Product?> FindProductAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> FindProductsAsync(
        int page,
        int pageSize,
        string? category,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<StockItem>> FindStockAsync(
        Guid? warehouseId,
        Guid? productId,
        int? minAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryMovement>> FindMovementsAsync(int page,
        int pageSize,
        CancellationToken cancellationToken);
}

