using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class OutboundOperationFactory : IInventoryOperationFactory
{
    public MovementType Type => MovementType.Shipment;

    public void ApplyToStock(StockItem stockItem, int quantity)
    {
        stockItem.ShipReserved(quantity);
    }

    public InventoryMovement CreateMovement(StockItem stockItem,
        int quantity,
        Guid userId,
        Guid? orderId = null)
    {
        return stockItem.RecordMovement(Type, quantity, userId, orderId);
    }
}

