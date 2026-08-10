namespace CustomerSupport.Domain.Plans;

public sealed class Plan
{
    private Plan(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; }

    public static Plan Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Plan ID cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Plan(id, name.Trim());
    }
}
