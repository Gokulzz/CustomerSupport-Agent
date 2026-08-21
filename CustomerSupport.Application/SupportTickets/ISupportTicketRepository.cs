using CustomerSupport.Domain.SupportTickets;

namespace CustomerSupport.Application.SupportTickets;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAndCustomerIdAsync(
        Guid ticketId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SupportTicket supportTicket,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
