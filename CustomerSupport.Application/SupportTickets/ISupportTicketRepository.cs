using CustomerSupport.Domain.SupportTickets;

namespace CustomerSupport.Application.SupportTickets;

public interface ISupportTicketRepository
{
    Task AddAsync(
        SupportTicket supportTicket,
        CancellationToken cancellationToken = default);
}
