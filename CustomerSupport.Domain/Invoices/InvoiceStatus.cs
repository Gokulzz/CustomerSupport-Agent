namespace CustomerSupport.Domain.Invoices;

public enum InvoiceStatus
{
    Unpaid = 1,
    Paid = 2,
    Refunded = 3,
    PaymentFailed = 4
}
