namespace InventoryReservation.Domain;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetForUpdateAsync(Guid id, CancellationToken ct);

    Task AddAsync(SalesOrder order, CancellationToken ct);
}

