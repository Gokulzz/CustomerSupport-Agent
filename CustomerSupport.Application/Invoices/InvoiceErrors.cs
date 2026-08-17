using CustomerSupport.Application.Results;

namespace CustomerSupport.Application.Invoices;

public static class InvoiceErrors
{
    public static Error NotFound { get; } = new(
        "Invoice.NotFound",
        "The invoice was not found.");
}
