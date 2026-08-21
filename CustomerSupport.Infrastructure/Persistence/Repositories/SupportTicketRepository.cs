using CustomerSupport.Application.SupportTickets;
using CustomerSupport.Domain.SupportTickets;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Infrastructure.Persistence.Repositories;

internal sealed class SupportTicketRepository(SupportDbContext dbContext)
    : ISupportTicketRepository
{
    public Task<SupportTicket?> GetByIdAndCustomerIdAsync(
        Guid ticketId,
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        dbContext.SupportTickets.SingleOrDefaultAsync(
            ticket => ticket.Id == ticketId && ticket.CustomerId == customerId,
            cancellationToken);

    public async Task AddAsync(
        SupportTicket supportTicket,
        CancellationToken cancellationToken = default)
    {
        dbContext.SupportTickets.Add(supportTicket);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
