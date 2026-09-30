namespace InventoryReservation.Domain.Tests;

internal sealed class NoOpStockOperation : IStockOperation
{
    public void ApplyToStock(StockItem stockItem, int quantity)
    {
    }
}

