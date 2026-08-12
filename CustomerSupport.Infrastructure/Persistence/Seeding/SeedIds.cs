namespace CustomerSupport.Infrastructure.Persistence.Seeding;

public static class SeedIds
{
    public static readonly Guid FreePlanId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid ProPlanId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid BusinessPlanId = Guid.Parse("10000000-0000-0000-0000-000000000003");

    public static readonly Guid AliceCustomerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid BobCustomerId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid CharlieCustomerId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    public static readonly Guid AliceSubscriptionId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid BobSubscriptionId = Guid.Parse("30000000-0000-0000-0000-000000000002");
    public static readonly Guid CharlieSubscriptionId = Guid.Parse("30000000-0000-0000-0000-000000000003");

    public static readonly Guid AliceRecentPaidInvoiceId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid AliceOldPaidInvoiceId = Guid.Parse("40000000-0000-0000-0000-000000000002");
    public static readonly Guid BobRefundedInvoiceId = Guid.Parse("40000000-0000-0000-0000-000000000003");
    public static readonly Guid BobUnpaidInvoiceId = Guid.Parse("40000000-0000-0000-0000-000000000004");
    public static readonly Guid BobFailedInvoiceId = Guid.Parse("40000000-0000-0000-0000-000000000005");

    public static readonly Guid AliceOpenSupportTicketId = Guid.Parse("50000000-0000-0000-0000-000000000001");
}
