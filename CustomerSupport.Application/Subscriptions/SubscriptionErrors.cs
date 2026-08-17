using CustomerSupport.Application.Results;

namespace CustomerSupport.Application.Subscriptions;

public static class SubscriptionErrors
{
    public static Error NotFound { get; } = new(
        "Subscription.NotFound",
        "A subscription was not found for the authenticated customer.");
}
