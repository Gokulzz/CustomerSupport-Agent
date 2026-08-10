namespace CustomerSupport.Domain.Invoices;

public sealed class Invoice
{
    private static readonly TimeSpan RefundWindow = TimeSpan.FromDays(14);

    private Invoice(
        Guid id,
        Guid customerId,
        string invoiceNumber,
        decimal amount,
        DateTimeOffset issuedAtUtc,
        InvoiceStatus status,
        DateTimeOffset? paidAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        Amount = amount;
        IssuedAtUtc = issuedAtUtc;
        Status = status;
        PaidAtUtc = paidAtUtc;
    }

    public Guid Id { get; }

    public Guid CustomerId { get; }

    public string InvoiceNumber { get; }

    public decimal Amount { get; }

    public DateTimeOffset IssuedAtUtc { get; }

    public InvoiceStatus Status { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public static Invoice Create(
        Guid id,
        Guid customerId,
        string invoiceNumber,
        decimal amount,
        DateTimeOffset issuedAtUtc,
        InvoiceStatus status = InvoiceStatus.Unpaid,
        DateTimeOffset? paidAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(id));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber);

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Invoice amount must be greater than zero.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        var issuedAt = issuedAtUtc.ToUniversalTime();
        var paidAt = paidAtUtc?.ToUniversalTime();
        var requiresPaymentTimestamp = status is InvoiceStatus.Paid or InvoiceStatus.Refunded;

        if (requiresPaymentTimestamp && paidAt is null)
        {
            throw new ArgumentException("A paid or refunded invoice requires a payment timestamp.", nameof(paidAtUtc));
        }

        if (!requiresPaymentTimestamp && paidAt is not null)
        {
            throw new ArgumentException("An unpaid invoice cannot have a payment timestamp.", nameof(paidAtUtc));
        }

        if (paidAt < issuedAt)
        {
            throw new ArgumentException("The payment timestamp cannot be before the invoice was issued.", nameof(paidAtUtc));
        }

        return new Invoice(
            id,
            customerId,
            invoiceNumber.Trim(),
            amount,
            issuedAt,
            status,
            paidAt);
    }

    public MarkInvoicePaidOutcome MarkPaid(DateTimeOffset paidAtUtc)
    {
        if (Status == InvoiceStatus.Paid)
        {
            return MarkInvoicePaidOutcome.AlreadyPaid;
        }

        if (Status == InvoiceStatus.Refunded)
        {
            return MarkInvoicePaidOutcome.InvoiceAlreadyRefunded;
        }

        var paidAt = paidAtUtc.ToUniversalTime();

        if (paidAt < IssuedAtUtc)
        {
            return MarkInvoicePaidOutcome.PaymentDateBeforeIssue;
        }

        Status = InvoiceStatus.Paid;
        PaidAtUtc = paidAt;

        return MarkInvoicePaidOutcome.Success;
    }

    public RefundEligibility CheckRefundEligibility(DateTimeOffset now)
    {
        if (Status == InvoiceStatus.Refunded)
        {
            return RefundEligibility.AlreadyRefunded;
        }

        if (Status != InvoiceStatus.Paid)
        {
            return RefundEligibility.InvoiceNotPaid;
        }

        var paidAt = PaidAtUtc
            ?? throw new InvalidOperationException("A paid invoice must have a payment timestamp.");
        var currentTime = now.ToUniversalTime();

        if (currentTime < paidAt)
        {
            return RefundEligibility.PaymentDateInFuture;
        }

        return currentTime - paidAt <= RefundWindow
            ? RefundEligibility.Eligible
            : RefundEligibility.RefundWindowExpired;
    }
}
