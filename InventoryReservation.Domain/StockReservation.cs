namespace InventoryReservation.Domain;

public sealed class StockReservation
{
    internal StockReservation(Guid orderId, SalesOrderItem item, StockItem stockItem)
    {
        if (item.ProductId != stockItem.ProductId)
        {
            throw new DomainRuleViolationException("Reservation product does not match order item.");
        }

        stockItem.Reserve(item.Quantity);
        Id = Guid.NewGuid();
        SalesOrderId = orderId;
        SalesOrderItemId = item.Id;
        StockItem = stockItem;
        StockItemId = stockItem.Id;
        Quantity = item.Quantity;
        CreatedAtUtc = DateTime.UtcNow;
        stockItem.RecordMovement(MovementType.Reservation, Quantity, Guid.Empty, orderId);
    }

    private StockReservation()
    {
    }

    public Guid Id { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid SalesOrderItemId { get; private set; }
    public Guid StockItemId { get; private set; }
    public StockItem StockItem { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;

    internal void Release()
    {
        EnsureActive();
        StockItem.ReleaseReservation(Quantity);
        StockItem.RecordMovement(MovementType.ReservationReleased, Quantity, Guid.Empty, SalesOrderId);
        IsActive = false;
    }

    public void Ship(IStockOperation shippingOperation)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(shippingOperation);
        var onHandBefore = StockItem.OnHandQuantity;
        var reservedBefore = StockItem.ReservedQuantity;

        shippingOperation.ApplyToStock(StockItem, Quantity);

        if (StockItem.OnHandQuantity != onHandBefore - Quantity ||
            StockItem.ReservedQuantity != reservedBefore - Quantity)
        {
            throw new DomainRuleViolationException("Shipment must consume the reserved quantity.");
        }
        IsActive = false;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainRuleViolationException("Reservation has already been completed.");
        }
    }
}

