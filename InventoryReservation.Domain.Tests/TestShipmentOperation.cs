namespace InventoryReservation.Domain.Tests;

internal sealed class TestShipmentOperation : IStockOperation
{
    public void ApplyToStock(StockItem stockItem, int quantity)
    {
        stockItem.ShipReserved(quantity);
    }
}

