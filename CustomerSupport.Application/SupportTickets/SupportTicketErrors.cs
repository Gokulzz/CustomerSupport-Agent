using CustomerSupport.Application.Results;

namespace CustomerSupport.Application.SupportTickets;

public static class SupportTicketErrors
{
    public static Error NotFound { get; } = new(
        "SupportTicket.NotFound",
        "The support ticket was not found.");

    public static Error AlreadyEscalated { get; } = new(
        "SupportTicket.AlreadyEscalated",
        "The support ticket has already been escalated.");

    public static Error AlreadyClosed { get; } = new(
        "SupportTicket.AlreadyClosed",
        "A closed support ticket cannot be escalated.");
}
