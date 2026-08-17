namespace CustomerSupport.Application.SupportTickets;

public sealed record SupportTicketDto(
    Guid Id,
    string Subject,
    string Description,
    string Status,
    DateTimeOffset OpenedAtUtc);
