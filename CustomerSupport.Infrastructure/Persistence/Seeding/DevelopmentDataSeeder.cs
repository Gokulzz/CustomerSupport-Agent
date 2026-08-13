using CustomerSupport.Domain.Customers;
using CustomerSupport.Domain.Invoices;
using CustomerSupport.Domain.Plans;
using CustomerSupport.Domain.Subscriptions;
using CustomerSupport.Domain.SupportTickets;
using CustomerSupport.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerSupport.Infrastructure.Persistence.Seeding;

public sealed class DevelopmentDataSeeder(
    SupportDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<DevelopmentDataSeeder> logger)
{
    private const string AliceEmail = "alice@clouddesk.test";
    private const string BobEmail = "bob@clouddesk.test";
    private const string CharlieEmail = "charlie@clouddesk.test";
    private const string AlicePassword = "AliceDev!2026";
    private const string BobPassword = "BobDev!2026";
    private const string CharliePassword = "CharlieDev!2026";

    private static readonly DateTimeOffset ReferenceDateUtc =
        new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPlansAsync(cancellationToken);
        await SeedCustomersAsync(cancellationToken);
        await SeedSubscriptionsAsync(cancellationToken);
        await SeedInvoicesAsync(cancellationToken);
        await SeedSupportTicketsAsync(cancellationToken);

        var addedEntityCount = await dbContext.SaveChangesAsync(cancellationToken);
        var addedIdentityUserCount = await SeedIdentityUsersAsync();

        logger.LogInformation(
            "Development data seeding completed. Added {AddedEntityCount} domain entities and {AddedIdentityUserCount} identity users.",
            addedEntityCount,
            addedIdentityUserCount);
    }

    private async Task SeedPlansAsync(CancellationToken cancellationToken)
    {
        var seedIds = new[]
        {
            SeedIds.FreePlanId,
            SeedIds.ProPlanId,
            SeedIds.BusinessPlanId
        };
        var existingIds = await dbContext.Plans
            .Where(plan => seedIds.Contains(plan.Id))
            .Select(plan => plan.Id)
            .ToHashSetAsync(cancellationToken);

        if (!existingIds.Contains(SeedIds.FreePlanId))
        {
            dbContext.Plans.Add(Plan.Create(SeedIds.FreePlanId, "Free"));
        }

        if (!existingIds.Contains(SeedIds.ProPlanId))
        {
            dbContext.Plans.Add(Plan.Create(SeedIds.ProPlanId, "Pro"));
        }

        if (!existingIds.Contains(SeedIds.BusinessPlanId))
        {
            dbContext.Plans.Add(Plan.Create(SeedIds.BusinessPlanId, "Business"));
        }
    }

    private async Task SeedCustomersAsync(CancellationToken cancellationToken)
    {
        var seedIds = new[]
        {
            SeedIds.AliceCustomerId,
            SeedIds.BobCustomerId,
            SeedIds.CharlieCustomerId
        };
        var existingIds = await dbContext.Customers
            .Where(customer => seedIds.Contains(customer.Id))
            .Select(customer => customer.Id)
            .ToHashSetAsync(cancellationToken);

        if (!existingIds.Contains(SeedIds.AliceCustomerId))
        {
            dbContext.Customers.Add(Customer.Create(SeedIds.AliceCustomerId, "Alice"));
        }

        if (!existingIds.Contains(SeedIds.BobCustomerId))
        {
            dbContext.Customers.Add(Customer.Create(SeedIds.BobCustomerId, "Bob"));
        }

        if (!existingIds.Contains(SeedIds.CharlieCustomerId))
        {
            dbContext.Customers.Add(Customer.Create(SeedIds.CharlieCustomerId, "Charlie"));
        }
    }

    private async Task SeedSubscriptionsAsync(CancellationToken cancellationToken)
    {
        var seedIds = new[]
        {
            SeedIds.AliceSubscriptionId,
            SeedIds.BobSubscriptionId,
            SeedIds.CharlieSubscriptionId
        };
        var existingIds = await dbContext.Subscriptions
            .Where(subscription => seedIds.Contains(subscription.Id))
            .Select(subscription => subscription.Id)
            .ToHashSetAsync(cancellationToken);

        if (!existingIds.Contains(SeedIds.AliceSubscriptionId))
        {
            dbContext.Subscriptions.Add(Subscription.Create(
                SeedIds.AliceSubscriptionId,
                SeedIds.AliceCustomerId,
                SeedIds.ProPlanId,
                SubscriptionStatus.Active,
                ReferenceDateUtc.AddDays(30)));
        }

        if (!existingIds.Contains(SeedIds.BobSubscriptionId))
        {
            dbContext.Subscriptions.Add(Subscription.Create(
                SeedIds.BobSubscriptionId,
                SeedIds.BobCustomerId,
                SeedIds.BusinessPlanId,
                SubscriptionStatus.Cancelled,
                renewsAtUtc: null));
        }

        if (!existingIds.Contains(SeedIds.CharlieSubscriptionId))
        {
            dbContext.Subscriptions.Add(Subscription.Create(
                SeedIds.CharlieSubscriptionId,
                SeedIds.CharlieCustomerId,
                SeedIds.FreePlanId,
                SubscriptionStatus.Expired,
                renewsAtUtc: null));
        }
    }

    private async Task SeedInvoicesAsync(CancellationToken cancellationToken)
    {
        var seedIds = new[]
        {
            SeedIds.AliceRecentPaidInvoiceId,
            SeedIds.AliceOldPaidInvoiceId,
            SeedIds.BobRefundedInvoiceId,
            SeedIds.BobUnpaidInvoiceId,
            SeedIds.BobFailedInvoiceId
        };
        var existingIds = await dbContext.Invoices
            .Where(invoice => seedIds.Contains(invoice.Id))
            .Select(invoice => invoice.Id)
            .ToHashSetAsync(cancellationToken);

        if (!existingIds.Contains(SeedIds.AliceRecentPaidInvoiceId))
        {
            dbContext.Invoices.Add(Invoice.Create(
                SeedIds.AliceRecentPaidInvoiceId,
                SeedIds.AliceCustomerId,
                "CD-2026-0801",
                49.00m,
                new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero),
                InvoiceStatus.Paid,
                ReferenceDateUtc.AddDays(-5)));
        }

        if (!existingIds.Contains(SeedIds.AliceOldPaidInvoiceId))
        {
            dbContext.Invoices.Add(Invoice.Create(
                SeedIds.AliceOldPaidInvoiceId,
                SeedIds.AliceCustomerId,
                "CD-2026-0701",
                49.00m,
                new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero),
                InvoiceStatus.Paid,
                ReferenceDateUtc.AddDays(-31)));
        }

        if (!existingIds.Contains(SeedIds.BobRefundedInvoiceId))
        {
            dbContext.Invoices.Add(Invoice.Create(
                SeedIds.BobRefundedInvoiceId,
                SeedIds.BobCustomerId,
                "CD-2026-0601",
                99.00m,
                new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero),
                InvoiceStatus.Refunded,
                new DateTimeOffset(2026, 6, 10, 12, 0, 0, TimeSpan.Zero)));
        }

        if (!existingIds.Contains(SeedIds.BobUnpaidInvoiceId))
        {
            dbContext.Invoices.Add(Invoice.Create(
                SeedIds.BobUnpaidInvoiceId,
                SeedIds.BobCustomerId,
                "CD-2026-0802",
                99.00m,
                new DateTimeOffset(2026, 8, 2, 12, 0, 0, TimeSpan.Zero),
                InvoiceStatus.Unpaid));
        }

        if (!existingIds.Contains(SeedIds.BobFailedInvoiceId))
        {
            dbContext.Invoices.Add(Invoice.Create(
                SeedIds.BobFailedInvoiceId,
                SeedIds.BobCustomerId,
                "CD-2026-0803",
                99.00m,
                new DateTimeOffset(2026, 8, 5, 12, 0, 0, TimeSpan.Zero),
                InvoiceStatus.PaymentFailed));
        }
    }

    private async Task SeedSupportTicketsAsync(CancellationToken cancellationToken)
    {
        var seedIds = new[] { SeedIds.AliceOpenSupportTicketId };
        var existingIds = await dbContext.SupportTickets
            .Where(ticket => seedIds.Contains(ticket.Id))
            .Select(ticket => ticket.Id)
            .ToHashSetAsync(cancellationToken);

        if (!existingIds.Contains(SeedIds.AliceOpenSupportTicketId))
        {
            dbContext.SupportTickets.Add(SupportTicket.Create(
                SeedIds.AliceOpenSupportTicketId,
                SeedIds.AliceCustomerId,
                "Project export is stuck",
                "The project export remains in progress and never completes.",
                new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero)));
        }
    }

    private async Task<int> SeedIdentityUsersAsync()
    {
        var addedUserCount = 0;

        addedUserCount += await SeedIdentityUserAsync(
            SeedIds.AliceUserId,
            SeedIds.AliceCustomerId,
            AliceEmail,
            AlicePassword);
        addedUserCount += await SeedIdentityUserAsync(
            SeedIds.BobUserId,
            SeedIds.BobCustomerId,
            BobEmail,
            BobPassword);
        addedUserCount += await SeedIdentityUserAsync(
            SeedIds.CharlieUserId,
            SeedIds.CharlieCustomerId,
            CharlieEmail,
            CharliePassword);

        return addedUserCount;
    }

    private async Task<int> SeedIdentityUserAsync(
        Guid userId,
        Guid customerId,
        string email,
        string password)
    {
        var userById = await userManager.FindByIdAsync(userId.ToString());
        var userByEmail = await userManager.FindByEmailAsync(email);
        var existingUser = userById ?? userByEmail;

        if (existingUser is not null)
        {
            if (existingUser.Id != userId ||
                existingUser.CustomerId != customerId ||
                !string.Equals(existingUser.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Development identity seed data conflicts with the existing user '{email}'.");
            }

            return 0;
        }

        var user = new ApplicationUser
        {
            Id = userId,
            CustomerId = customerId,
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => $"{error.Code}: {error.Description}"));

            throw new InvalidOperationException(
                $"Could not create development identity user '{email}': {errors}");
        }

        return 1;
    }
}
