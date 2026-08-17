using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Results;

namespace CustomerSupport.Application.Subscriptions;

public sealed class GetMySubscription(
    ICurrentCustomer currentCustomer,
    ISubscriptionQuery subscriptionQuery)
{
    public async Task<Result<SubscriptionDto>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionQuery.GetByCustomerIdAsync(
            currentCustomer.CustomerId,
            cancellationToken);

        return subscription is null
            ? Result<SubscriptionDto>.Failure(SubscriptionErrors.NotFound)
            : Result<SubscriptionDto>.Success(subscription);
    }
}
