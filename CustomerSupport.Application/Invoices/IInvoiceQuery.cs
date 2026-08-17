namespace CustomerSupport.Application.Invoices;

public interface IInvoiceQuery
{
    Task<IReadOnlyList<InvoiceDto>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
