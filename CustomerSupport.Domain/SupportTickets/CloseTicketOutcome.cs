namespace CustomerSupport.Domain.SupportTickets;

public enum CloseTicketOutcome
{
    Success = 1,
    AlreadyClosed = 2,
    CloseDateBeforeOpen = 3
}
