namespace CustomerSupport.Domain.SupportTickets;

public sealed class SupportTicket
{
    private SupportTicket(
        Guid id,
        Guid customerId,
        string subject,
        string description,
        DateTimeOffset openedAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        Subject = subject;
        Description = description;
        OpenedAtUtc = openedAtUtc;
        Status = TicketStatus.Open;
    }

    public Guid Id { get; }

    public Guid CustomerId { get; }

    public string Subject { get; }

    public string Description { get; }

    public TicketStatus Status { get; private set; }

    public DateTimeOffset OpenedAtUtc { get; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public static SupportTicket Create(
        Guid id,
        Guid customerId,
        string subject,
        string description,
        DateTimeOffset openedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Support ticket ID cannot be empty.", nameof(id));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new SupportTicket(
            id,
            customerId,
            subject.Trim(),
            description.Trim(),
            openedAtUtc.ToUniversalTime());
    }

    public CloseTicketOutcome Close(DateTimeOffset closedAtUtc)
    {
        if (Status == TicketStatus.Closed)
        {
            return CloseTicketOutcome.AlreadyClosed;
        }

        var closedAt = closedAtUtc.ToUniversalTime();

        if (closedAt < OpenedAtUtc)
        {
            return CloseTicketOutcome.CloseDateBeforeOpen;
        }

        Status = TicketStatus.Closed;
        ClosedAtUtc = closedAt;

        return CloseTicketOutcome.Success;
    }
}
