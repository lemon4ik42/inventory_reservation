namespace InventoryReservation.Domain;

public sealed class InventoryMovement
{
    public InventoryMovement(
        Guid stockItemId,
        MovementType type,
        int quantity,
        Guid userId,
        Guid? orderId = null)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleViolationException("Movement quantity must be positive.");
        }

        if ((type is MovementType.Reservation or MovementType.ReservationReleased or
            MovementType.Shipment) && orderId is null)
        {
            throw new DomainRuleViolationException("Order movement must reference an order.");
        }

        Id = Guid.NewGuid();
        StockItemId = stockItemId;
        Type = type;
        Quantity = quantity;
        ApplicationUserId = userId;
        SalesOrderId = orderId;
        OccurredAtUtc = DateTime.UtcNow;
    }

    private InventoryMovement()
    {
    }

    public Guid Id { get; private set; }
    public Guid StockItemId { get; private set; }
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public Guid ApplicationUserId { get; private set; }
    public Guid? SalesOrderId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
}

