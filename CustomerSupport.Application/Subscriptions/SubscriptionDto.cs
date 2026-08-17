namespace CustomerSupport.Application.Subscriptions;

public sealed record SubscriptionDto(
    Guid Id,
    string PlanName,
    string Status,
    DateTimeOffset? RenewsAtUtc,
    bool IsActive);
