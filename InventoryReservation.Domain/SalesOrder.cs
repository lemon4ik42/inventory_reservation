namespace InventoryReservation.Domain;

public sealed class SalesOrder
{
    public SalesOrder(Guid customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Number = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Id.ToString()[..8].ToUpperInvariant()}";
    }

    private SalesOrder()
    {
    }

    public Guid Id { get; private set; }
    public string Number { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();
    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    private readonly List<SalesOrderItem> _items = [];
    private readonly List<StockReservation> _reservations = [];

    public void AddItem(Guid productId, int quantity)
    {
        EnsureStatus(OrderStatus.Draft);
        var existing = _items.SingleOrDefault(item => item.ProductId == productId);
        if (existing is null)
        {
            _items.Add(new SalesOrderItem(Id, productId, quantity));
        }
        else
        {
            existing.IncreaseQuantity(quantity);
        }

        Touch();
    }

    public void Confirm()
    {
        EnsureStatus(OrderStatus.Draft);
        if (_items.Count == 0)
        {
            throw new DomainRuleViolationException("An empty order cannot be confirmed.");
        }

        Status = OrderStatus.Confirmed;
        Touch();
    }

    public void ReserveItem(SalesOrderItem item, StockItem stockItem)
    {
        EnsureStatus(OrderStatus.Confirmed);
        if (!_items.Contains(item) || _reservations.Any(reservation => reservation.SalesOrderItemId == item.Id))
        {
            throw new DomainRuleViolationException("Order item is unknown or already reserved.");
        }

        _reservations.Add(new StockReservation(Id, item, stockItem));
        Touch();
    }

    public void MarkReserved()
    {
        EnsureStatus(OrderStatus.Confirmed);
        if (_items.Any(item => _reservations
            .Where(reservation => reservation.SalesOrderItemId == item.Id && reservation.IsActive)
            .Sum(reservation => reservation.Quantity) != item.Quantity))
        {
            throw new DomainRuleViolationException("Every order item must be fully reserved.");
        }

        Status = OrderStatus.Reserved;
        Touch();
    }

    public void Ship()
    {
        EnsureStatus(OrderStatus.Reserved);
        if (_reservations.Any(reservation => reservation.IsActive))
        {
            throw new DomainRuleViolationException("Reservations must be shipped before completing order.");
        }

        Status = OrderStatus.Shipped;
        Touch();
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Cancelled)
        {
            throw new DomainRuleViolationException("Shipped or cancelled order cannot be cancelled.");
        }

        foreach (var reservation in _reservations.Where(reservation => reservation.IsActive))
        {
            reservation.Release();
        }

        Status = OrderStatus.Cancelled;
        Touch();
    }

    public void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
        {
            throw new DomainRuleViolationException($"Order must be {expected}, but is {Status}.");
        }
    }

    private void Touch()
    {
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}

