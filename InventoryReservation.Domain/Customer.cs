namespace InventoryReservation.Domain;

public sealed class Customer
{
    public Customer(string name)
    {
        Id = Guid.NewGuid();
        Rename(name);
    }

    private Customer()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleViolationException("Name is required.");
        }

        Name = name.Trim();
    }
}

