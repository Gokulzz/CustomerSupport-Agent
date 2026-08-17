namespace CustomerSupport.Application.Subscriptions;

public interface ISubscriptionQuery
{
    Task<SubscriptionDto?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
