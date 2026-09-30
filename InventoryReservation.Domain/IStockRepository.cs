namespace InventoryReservation.Domain;

public interface IStockRepository
{
    Task<Warehouse?> GetWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StockItem>> GetAvailableForProductAsync(Guid productId,
        CancellationToken cancellationToken);
}

