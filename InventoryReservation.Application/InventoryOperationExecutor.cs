using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class InventoryOperationExecutor
{
    public InventoryOperationExecutor(IEnumerable<IInventoryOperationFactory> factories)
    {
        _factories = factories.ToDictionary(factory => factory.Type);
    }

    private readonly IReadOnlyDictionary<MovementType, IInventoryOperationFactory> _factories;

    public void Execute(
        MovementType type,
        StockItem stockItem,
        int quantity,
        Guid userId,
        Guid? orderId = null)
    {
        var factory = GetFactory(type);
        if (type == MovementType.Shipment && orderId is null)
        {
            throw new DomainRuleViolationException("Shipment must reference an order.");
        }
        factory.ApplyToStock(stockItem, quantity);
        factory.CreateMovement(stockItem, quantity, userId, orderId);
    }

    public void Ship(StockReservation reservation, Guid userId)
    {
        var factory = GetFactory(MovementType.Shipment);
        reservation.Ship(factory);
        factory.CreateMovement(
            reservation.StockItem,
            reservation.Quantity,
            userId,
            reservation.SalesOrderId);
    }

    private IInventoryOperationFactory GetFactory(MovementType type)
    {
        if (!_factories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"No factory registered for {type}.");
        }

        return factory;
    }

}

