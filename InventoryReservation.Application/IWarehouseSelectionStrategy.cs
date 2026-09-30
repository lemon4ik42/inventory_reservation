using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public interface IWarehouseSelectionStrategy
{
    StockItem Select(IReadOnlyCollection<StockItem> candidates);
}

