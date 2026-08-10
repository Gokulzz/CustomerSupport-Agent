using CustomerSupport.Domain.Subscriptions;
using Xunit;

namespace CustomerSupport.Domain.Tests.Subscriptions;

public sealed class SubscriptionTests
{
    private static readonly Guid SubscriptionId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid CustomerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid PlanId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    [Fact]
    public void Create_ActiveSubscriptionWithRenewal_CreatesActiveSubscription()
    {
        var renewsAtUtc = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

        var subscription = Subscription.Create(
            SubscriptionId,
            CustomerId,
            PlanId,
            SubscriptionStatus.Active,
            renewsAtUtc);

        Assert.True(subscription.IsActive);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(renewsAtUtc, subscription.RenewsAtUtc);
    }

    [Fact]
    public void Create_ActiveSubscriptionWithoutRenewal_Throws()
    {
        Assert.Throws<ArgumentException>(() => Subscription.Create(
            SubscriptionId,
            CustomerId,
            PlanId,
            SubscriptionStatus.Active,
            renewsAtUtc: null));
    }

    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        Assert.Throws<ArgumentException>(() => Subscription.Create(
            SubscriptionId,
            Guid.Empty,
            PlanId,
            SubscriptionStatus.Inactive,
            renewsAtUtc: null));
    }
}
