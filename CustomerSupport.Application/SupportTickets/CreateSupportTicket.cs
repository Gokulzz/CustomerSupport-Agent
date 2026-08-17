using CustomerSupport.Application.Authentication;
using CustomerSupport.Domain.SupportTickets;

namespace CustomerSupport.Application.SupportTickets;

public sealed class CreateSupportTicket(
    ICurrentCustomer currentCustomer,
    ISupportTicketRepository supportTicketRepository,
    TimeProvider timeProvider)
{
    public async Task<SupportTicketDto> ExecuteAsync(
        CreateSupportTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var supportTicket = SupportTicket.Create(
            Guid.NewGuid(),
            currentCustomer.CustomerId,
            request.Subject,
            request.Description,
            timeProvider.GetUtcNow());

        await supportTicketRepository.AddAsync(supportTicket, cancellationToken);

        return new SupportTicketDto(
            supportTicket.Id,
            supportTicket.Subject,
            supportTicket.Description,
            supportTicket.Status.ToString(),
            supportTicket.OpenedAtUtc);
    }
}
