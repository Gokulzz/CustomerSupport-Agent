namespace CustomerSupport.Domain.Subscriptions;

public sealed class Subscription
{
    private Subscription(
        Guid id,
        Guid customerId,
        Guid planId,
        SubscriptionStatus status,
        DateTimeOffset? renewsAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        PlanId = planId;
        Status = status;
        RenewsAtUtc = renewsAtUtc;
    }

    public Guid Id { get; }

    public Guid CustomerId { get; }

    public Guid PlanId { get; }

    public SubscriptionStatus Status { get; }

    public DateTimeOffset? RenewsAtUtc { get; }

    public bool IsActive => Status == SubscriptionStatus.Active;

    public static Subscription Create(
        Guid id,
        Guid customerId,
        Guid planId,
        SubscriptionStatus status,
        DateTimeOffset? renewsAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Subscription ID cannot be empty.", nameof(id));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }

        if (planId == Guid.Empty)
        {
            throw new ArgumentException("Plan ID cannot be empty.", nameof(planId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (status == SubscriptionStatus.Active && renewsAtUtc is null)
        {
            throw new ArgumentException("An active subscription requires a renewal timestamp.", nameof(renewsAtUtc));
        }

        return new Subscription(
            id,
            customerId,
            planId,
            status,
            renewsAtUtc?.ToUniversalTime());
    }
}
