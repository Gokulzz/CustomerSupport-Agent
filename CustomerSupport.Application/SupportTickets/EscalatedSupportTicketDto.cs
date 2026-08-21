namespace CustomerSupport.Application.SupportTickets;

public sealed record EscalatedSupportTicketDto(
    Guid Id,
    string Status,
    DateTimeOffset EscalatedAtUtc);
