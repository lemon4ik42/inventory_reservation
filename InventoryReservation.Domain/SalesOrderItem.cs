namespace InventoryReservation.Domain;

public sealed class SalesOrderItem
{
    internal SalesOrderItem(Guid orderId, Guid productId, int quantity)
    {
        Id = Guid.NewGuid();
        SalesOrderId = orderId;
        ProductId = productId;
        IncreaseQuantity(quantity);
    }

    private SalesOrderItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleViolationException("Quantity must be positive.");
        }

        if (quantity > int.MaxValue - Quantity)
        {
            throw new DomainRuleViolationException("Order quantity exceeds supported maximum.");
        }

        Quantity += quantity;
    }
}

