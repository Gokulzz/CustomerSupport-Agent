using CustomerSupport.Application.Subscriptions;
using CustomerSupport.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Infrastructure.Persistence.Queries;

internal sealed class SubscriptionQuery(SupportDbContext dbContext) : ISubscriptionQuery
{
   
    public async Task<SubscriptionDto?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var subscriptionRecord = await (
            from subscription in dbContext.Subscriptions.AsNoTracking()
            join plan in dbContext.Plans.AsNoTracking()
                on subscription.PlanId equals plan.Id
            where subscription.CustomerId == customerId
            select new
            {
                subscription.Id,
                PlanName = plan.Name,
                subscription.Status,
                subscription.RenewsAtUtc,
            }
        ).SingleOrDefaultAsync(cancellationToken);
      

        return subscriptionRecord is null
            ? null
            : new SubscriptionDto(
                subscriptionRecord.Id,
                subscriptionRecord.PlanName,
                subscriptionRecord.Status.ToString(),
                subscriptionRecord.RenewsAtUtc,
                subscriptionRecord.Status == SubscriptionStatus.Active);
    }
}
