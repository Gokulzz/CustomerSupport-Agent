using CustomerSupport.Domain.Invoices;

namespace CustomerSupport.Application.Invoices;

public interface IInvoiceQuery
{
    Task<IReadOnlyList<InvoiceDto>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAndCustomerIdAsync(
        Guid invoiceId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
