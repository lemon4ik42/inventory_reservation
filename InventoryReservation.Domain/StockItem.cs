namespace InventoryReservation.Domain;

public sealed class StockItem
{
    public StockItem(Product product, Warehouse warehouse)
    {
        product.EnsureActive();
        Id = Guid.NewGuid();
        Product = product;
        ProductId = product.Id;
        Warehouse = warehouse;
        WarehouseId = warehouse.Id;
        warehouse.RegisterStock(this);
    }

    private StockItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;
    public int OnHandQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => OnHandQuantity - ReservedQuantity;

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();
    public IReadOnlyCollection<InventoryMovement> Movements => _movements.AsReadOnly();

    private readonly List<InventoryMovement> _movements = [];

    public void Receive(int quantity)
    {
        Product.EnsureActive();
        EnsurePositive(quantity);
        if (quantity > int.MaxValue - OnHandQuantity)
        {
            throw new DomainRuleViolationException("Stock quantity exceeds supported maximum.");
        }

        OnHandQuantity += quantity;
        Touch();
    }

    public void Reserve(int quantity)
    {
        Product.EnsureActive();
        EnsurePositive(quantity);
        if (quantity > AvailableQuantity)
        {
            throw new DomainRuleViolationException("Insufficient available stock.");
        }

        ReservedQuantity += quantity;
        Touch();
    }

    public void ReleaseReservation(int quantity)
    {
        EnsureReserved(quantity);
        ReservedQuantity -= quantity;
        Touch();
    }

    public void ShipReserved(int quantity)
    {
        EnsureReserved(quantity);
        ReservedQuantity -= quantity;
        OnHandQuantity -= quantity;
        Touch();
    }

    public InventoryMovement RecordMovement(MovementType type,
        int quantity,
        Guid userId,
        Guid? orderId = null)
    {
        var movement = new InventoryMovement(Id, type, quantity, userId, orderId);
        _movements.Add(movement);
        return movement;
    }

    private void EnsureReserved(int quantity)
    {
        EnsurePositive(quantity);
        if (quantity > ReservedQuantity)
        {
            throw new DomainRuleViolationException("Quantity exceeds reserved stock.");
        }
    }

    private void Touch()
    {
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleViolationException("Quantity must be positive.");
        }
    }
}

