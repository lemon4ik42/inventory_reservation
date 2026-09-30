namespace InventoryReservation.Domain;

public sealed class ApplicationUser
{
    public ApplicationUser(string userName)
    {
        Id = Guid.NewGuid();
        Rename(userName);
    }

    private ApplicationUser()
    {
    }

    public Guid Id { get; private set; }
    public string UserName { get; private set; } = null!;

    public void Rename(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainRuleViolationException("UserName is required.");
        }

        UserName = userName.Trim();
    }
}

