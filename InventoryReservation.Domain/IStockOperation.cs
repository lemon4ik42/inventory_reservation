namespace InventoryReservation.Domain;

public interface IStockOperation
{
    void ApplyToStock(StockItem stockItem, int quantity);
}

