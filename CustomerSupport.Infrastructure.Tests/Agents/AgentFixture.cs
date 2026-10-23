using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Invoices;
using CustomerSupport.Application.Subscriptions;
using CustomerSupport.Application.SupportTickets;
using CustomerSupport.Domain.Invoices;
using CustomerSupport.Domain.SupportTickets;
using CustomerSupport.Infrastructure.Agents;

namespace CustomerSupport.Infrastructure.Tests.Agents;

internal sealed class AgentFixture : ICurrentCustomer, ISubscriptionQuery, IInvoiceQuery, ISupportTicketRepository
{
    public static readonly DateTimeOffset Now = new(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);
    public Guid CustomerId { get; } = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public Guid? QueriedCustomerId { get; private set; }
    public Guid? QueriedResourceId { get; private set; }
    public CancellationToken ReceivedCancellationToken { get; private set; }
    public SubscriptionDto? Subscription { get; set; }
    public Invoice? Invoice { get; set; }
    public SupportTicket? Ticket { get; set; }
    public IReadOnlyList<InvoiceDto> Invoices { get; set; } = [];
    public int Reads { get; private set; }
    public int Creates { get; private set; }
    public int Saves { get; private set; }
    public Exception? ReadFailure { get; set; }

    public AgentToolDispatcher Dispatcher() => new(
        new GetMySubscription(this, this), new GetMyInvoices(this, this),
        new CheckRefundEligibility(this, this, new FixedClock()),
        new CreateSupportTicket(this, this, new FixedClock()),
        new EscalateSupportTicket(this, this, new FixedClock()));

    Task<SubscriptionDto?> ISubscriptionQuery.GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Capture(customerId, cancellationToken);
        return Task.FromResult(Subscription);
    }

    Task<IReadOnlyList<InvoiceDto>> IInvoiceQuery.GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Capture(customerId, cancellationToken);
        return Task.FromResult(Invoices);
    }

    public Task<Invoice?> GetByIdAndCustomerIdAsync(Guid invoiceId, Guid customerId, CancellationToken cancellationToken)
    {
        Capture(customerId, cancellationToken);
        QueriedResourceId = invoiceId;
        return Task.FromResult(Invoice?.Id == invoiceId && Invoice.CustomerId == customerId ? Invoice : null);
    }

    Task<SupportTicket?> ISupportTicketRepository.GetByIdAndCustomerIdAsync(Guid ticketId, Guid customerId, CancellationToken cancellationToken)
    {
        Capture(customerId, cancellationToken);
        QueriedResourceId = ticketId;
        return Task.FromResult(Ticket?.Id == ticketId && Ticket.CustomerId == customerId ? Ticket : null);
    }

    public Task AddAsync(SupportTicket ticket, CancellationToken cancellationToken)
    {
        Ticket = ticket;
        ReceivedCancellationToken = cancellationToken;
        Creates++;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        Saves++;
        return Task.CompletedTask;
    }

    private void Capture(Guid customerId, CancellationToken token)
    {
        if (ReadFailure is not null) throw ReadFailure;
        QueriedCustomerId = customerId;
        ReceivedCancellationToken = token;
        Reads++;
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
