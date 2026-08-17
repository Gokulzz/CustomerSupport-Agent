using CustomerSupport.Application.Authentication;

namespace CustomerSupport.Application.Invoices;

public sealed class GetMyInvoices(
    ICurrentCustomer currentCustomer,
    IInvoiceQuery invoiceQuery)
{
    public Task<IReadOnlyList<InvoiceDto>> ExecuteAsync(
        CancellationToken cancellationToken = default) =>
        invoiceQuery.GetByCustomerIdAsync(
            currentCustomer.CustomerId,
            cancellationToken);
}
