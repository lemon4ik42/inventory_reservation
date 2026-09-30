using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class InboundOperationFactory : IInventoryOperationFactory
{
    public MovementType Type => MovementType.Receipt;

    public void ApplyToStock(StockItem stockItem, int quantity)
    {
        stockItem.Receive(quantity);
    }

    public InventoryMovement CreateMovement(StockItem stockItem,
        int quantity,
        Guid userId,
        Guid? orderId = null)
    {
        return stockItem.RecordMovement(Type, quantity, userId, orderId);
    }
}

