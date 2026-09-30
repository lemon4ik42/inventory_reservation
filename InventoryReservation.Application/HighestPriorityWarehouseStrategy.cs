using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class HighestPriorityWarehouseStrategy : IWarehouseSelectionStrategy
{
    public StockItem Select(IReadOnlyCollection<StockItem> candidates)
    {
        return candidates
            .OrderByDescending(item => item.Warehouse.Priority)
            .ThenByDescending(item => item.AvailableQuantity)
            .FirstOrDefault()
            ?? throw new DomainRuleViolationException("No stock available.");
    }
}

