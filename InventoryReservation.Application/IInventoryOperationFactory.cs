using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public interface IInventoryOperationFactory : IStockOperation
{
    MovementType Type { get; }

    InventoryMovement CreateMovement(StockItem stockItem, int quantity, Guid userId, Guid? orderId = null);
}

