using CustomerSupport.Application.Invoices;
using CustomerSupport.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Infrastructure.Persistence.Queries;

internal sealed class InvoiceQuery(SupportDbContext dbContext) : IInvoiceQuery
{
    public async Task<IReadOnlyList<InvoiceDto>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.CustomerId == customerId)
            .OrderByDescending(invoice => invoice.IssuedAtUtc)
            .ThenBy(invoice => invoice.InvoiceNumber)
            .Select(invoice => new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.Amount,
                invoice.Status,
                invoice.IssuedAtUtc,
                invoice.PaidAtUtc
            })
            .ToListAsync(cancellationToken);

        return invoices
            .Select(invoice => new InvoiceDto(
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.Amount,
                invoice.Status.ToString(),
                invoice.IssuedAtUtc,
                invoice.PaidAtUtc))
            .ToArray();
    }

    public Task<Invoice?> GetByIdAndCustomerIdAsync(
        Guid invoiceId,
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        dbContext.Invoices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                invoice => invoice.Id == invoiceId && invoice.CustomerId == customerId,
                cancellationToken);
}
