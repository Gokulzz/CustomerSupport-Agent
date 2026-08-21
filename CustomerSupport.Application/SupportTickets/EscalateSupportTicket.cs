using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Results;
using CustomerSupport.Domain.SupportTickets;

namespace CustomerSupport.Application.SupportTickets;

public sealed class EscalateSupportTicket(
    ICurrentCustomer currentCustomer,
    ISupportTicketRepository supportTicketRepository,
    TimeProvider timeProvider)
{
    public async Task<Result<EscalatedSupportTicketDto>> ExecuteAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var supportTicket = await supportTicketRepository.GetByIdAndCustomerIdAsync(
            ticketId,
            currentCustomer.CustomerId,
            cancellationToken);

        if (supportTicket is null)
        {
            return Result<EscalatedSupportTicketDto>.Failure(SupportTicketErrors.NotFound);
        }

        var outcome = supportTicket.Escalate(timeProvider.GetUtcNow());

        if (outcome == EscalateTicketOutcome.AlreadyEscalated)
        {
            return Result<EscalatedSupportTicketDto>.Failure(
                SupportTicketErrors.AlreadyEscalated);
        }

        if (outcome == EscalateTicketOutcome.AlreadyClosed)
        {
            return Result<EscalatedSupportTicketDto>.Failure(
                SupportTicketErrors.AlreadyClosed);
        }

        if (outcome != EscalateTicketOutcome.Success)
        {
            throw new InvalidOperationException(
                $"Unhandled support ticket escalation outcome '{outcome}'.");
        }

        await supportTicketRepository.SaveChangesAsync(cancellationToken);

        var escalatedAtUtc = supportTicket.EscalatedAtUtc
            ?? throw new InvalidOperationException(
                "A successfully escalated support ticket requires an escalation timestamp.");

        return Result<EscalatedSupportTicketDto>.Success(
            new EscalatedSupportTicketDto(
                supportTicket.Id,
                supportTicket.Status.ToString(),
                escalatedAtUtc));
    }
}
