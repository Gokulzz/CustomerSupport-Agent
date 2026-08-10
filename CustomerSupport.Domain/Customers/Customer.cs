namespace CustomerSupport.Domain.Customers;

public sealed class Customer
{
    private Customer(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; }

    public static Customer Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer(id, name.Trim());
    }
}
