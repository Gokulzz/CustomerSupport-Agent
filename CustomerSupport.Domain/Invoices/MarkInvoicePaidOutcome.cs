namespace CustomerSupport.Domain.Invoices;

public enum MarkInvoicePaidOutcome
{
    Success = 1,
    AlreadyPaid = 2,
    InvoiceAlreadyRefunded = 3,
    PaymentDateBeforeIssue = 4
}
