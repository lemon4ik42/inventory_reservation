using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class HighestAvailableStockStrategy : IWarehouseSelectionStrategy
{
    public StockItem Select(IReadOnlyCollection<StockItem> candidates)
    {
        return candidates.OrderByDescending(item => item.AvailableQuantity).FirstOrDefault()
            ?? throw new DomainRuleViolationException("No stock available.");
    }
}

