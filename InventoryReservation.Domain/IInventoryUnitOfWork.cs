namespace InventoryReservation.Domain;

public interface IInventoryUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}

