using CustomerSupport.Application.SupportTickets;
using CustomerSupport.Domain.SupportTickets;

namespace CustomerSupport.Infrastructure.Persistence.Repositories;

internal sealed class SupportTicketRepository(SupportDbContext dbContext)
    : ISupportTicketRepository
{
    public async Task AddAsync(
        SupportTicket supportTicket,
        CancellationToken cancellationToken = default)
    {
        dbContext.SupportTickets.Add(supportTicket);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
