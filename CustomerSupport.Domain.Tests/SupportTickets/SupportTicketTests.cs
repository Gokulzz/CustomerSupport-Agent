using CustomerSupport.Domain.SupportTickets;
using Xunit;

namespace CustomerSupport.Domain.Tests.SupportTickets;

public sealed class SupportTicketTests
{
    private static readonly Guid TicketId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid CustomerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset OpenedAtUtc = new(2026, 8, 8, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidDetails_OpensTicket()
    {
        var ticket = CreateTicket();

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(OpenedAtUtc, ticket.OpenedAtUtc);
        Assert.Null(ticket.ClosedAtUtc);
    }

    [Fact]
    public void Close_WhenOpen_ClosesTicket()
    {
        var ticket = CreateTicket();
        var closedAtUtc = OpenedAtUtc.AddHours(2);

        var outcome = ticket.Close(closedAtUtc);

        Assert.Equal(CloseTicketOutcome.Success, outcome);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Equal(closedAtUtc, ticket.ClosedAtUtc);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_RejectsTransition()
    {
        var ticket = CreateTicket();
        var originalClosedAtUtc = OpenedAtUtc.AddHours(2);
        ticket.Close(originalClosedAtUtc);

        var outcome = ticket.Close(originalClosedAtUtc.AddHours(1));

        Assert.Equal(CloseTicketOutcome.AlreadyClosed, outcome);
        Assert.Equal(originalClosedAtUtc, ticket.ClosedAtUtc);
    }

    [Fact]
    public void Close_BeforeTicketWasOpened_RejectsTransition()
    {
        var ticket = CreateTicket();

        var outcome = ticket.Close(OpenedAtUtc.AddMinutes(-1));

        Assert.Equal(CloseTicketOutcome.CloseDateBeforeOpen, outcome);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.ClosedAtUtc);
    }

    private static SupportTicket CreateTicket() =>
        SupportTicket.Create(
            TicketId,
            CustomerId,
            "Unable to export project",
            "Project export remains stuck in progress.",
            OpenedAtUtc);
}
