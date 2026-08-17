using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Results;
using CustomerSupport.Domain.Invoices;

namespace CustomerSupport.Application.Invoices;

public sealed class CheckRefundEligibility(
    ICurrentCustomer currentCustomer,
    IInvoiceQuery invoiceQuery,
    TimeProvider timeProvider)
{
    public async Task<Result<RefundEligibilityDto>> ExecuteAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceQuery.GetByIdAndCustomerIdAsync(
            invoiceId,
            currentCustomer.CustomerId,
            cancellationToken);

        if (invoice is null)
        {
            return Result<RefundEligibilityDto>.Failure(InvoiceErrors.NotFound);
        }

        var eligibility = invoice.CheckRefundEligibility(timeProvider.GetUtcNow());
        var response = new RefundEligibilityDto(
            invoice.Id,
            eligibility == RefundEligibility.Eligible,
            eligibility.ToString());

        return Result<RefundEligibilityDto>.Success(response);
    }
}
