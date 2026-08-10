namespace CustomerSupport.Domain.Invoices;

public enum RefundEligibility
{
    Eligible = 1,
    InvoiceNotPaid = 2,
    AlreadyRefunded = 3,
    PaymentDateInFuture = 4,
    RefundWindowExpired = 5
}
