namespace InventoryReservation.Domain;

public sealed class Product
{
    public Product(string sku, string name, string category, Guid supplierId)
    {
        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleViolationException("SKU and name are required.");
        }

        Id = Guid.NewGuid();
        Sku = sku.Trim();
        Name = name.Trim();
        Category = category?.Trim() ?? string.Empty;
        SupplierId = supplierId;
    }

    private Product()
    {
    }

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public Guid SupplierId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainRuleViolationException("Inactive product cannot be stocked or ordered.");
        }
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleViolationException("Product name is required.");
        }

        Name = name.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

