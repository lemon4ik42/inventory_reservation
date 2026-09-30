namespace InventoryReservation.Domain;

public sealed class Warehouse
{
    public Warehouse(string name, string address, int priority)
    {
        Rename(name);
        Id = Guid.NewGuid();
        Address = address?.Trim() ?? string.Empty;
        Priority = priority;
    }

    private Warehouse()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public int Priority { get; private set; }
    public IReadOnlyCollection<StockItem> StockItems => _stockItems.AsReadOnly();
    private readonly List<StockItem> _stockItems = [];

    public StockItem GetOrCreateStock(Product product)
    {
        product.EnsureActive();
        var existing = _stockItems.SingleOrDefault(item => item.ProductId == product.Id);
        if (existing is not null)
        {
            return existing;
        }

        var item = new StockItem(product, this);
        return item;
    }

    internal void RegisterStock(StockItem stockItem)
    {
        if (_stockItems.Any(item => item.ProductId == stockItem.ProductId))
        {
            throw new DomainRuleViolationException("Warehouse already stocks this product.");
        }

        _stockItems.Add(stockItem);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleViolationException("Warehouse name is required.");
        }

        Name = name.Trim();
    }

    public void ChangePriority(int priority)
    {
        Priority = priority;
    }
}

