using CustomerSupport.Domain.Invoices;
using Xunit;

namespace CustomerSupport.Domain.Tests.Invoices;

public sealed class InvoiceTests
{
    private static readonly Guid InvoiceId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid CustomerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset IssuedAtUtc = new(2026, 8, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidDetails_CreatesUnpaidInvoice()
    {
        var invoice = CreateInvoice();

        Assert.Equal(InvoiceId, invoice.Id);
        Assert.Equal(CustomerId, invoice.CustomerId);
        Assert.Equal("INV-1001", invoice.InvoiceNumber);
        Assert.Equal(125.50m, invoice.Amount);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Null(invoice.PaidAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveAmount_Throws(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Invoice.Create(
            InvoiceId,
            CustomerId,
            "INV-1001",
            amount,
            IssuedAtUtc));
    }

    [Fact]
    public void Create_PaidInvoiceWithoutPaymentTimestamp_Throws()
    {
        Assert.Throws<ArgumentException>(() => Invoice.Create(
            InvoiceId,
            CustomerId,
            "INV-1001",
            125.50m,
            IssuedAtUtc,
            InvoiceStatus.Paid));
    }

    [Fact]
    public void MarkPaid_FromUnpaid_MarksInvoicePaid()
    {
        var invoice = CreateInvoice();
        var paidAtUtc = IssuedAtUtc.AddDays(1);

        var outcome = invoice.MarkPaid(paidAtUtc);

        Assert.Equal(MarkInvoicePaidOutcome.Success, outcome);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(paidAtUtc, invoice.PaidAtUtc);
    }

    [Fact]
    public void MarkPaid_WithTimestampBeforeIssue_RejectsTransition()
    {
        var invoice = CreateInvoice();
        // Attempt to mark the invoice as paid with a timestamp before the issued date
        var outcome = invoice.MarkPaid(IssuedAtUtc.AddMinutes(-1));

        Assert.Equal(MarkInvoicePaidOutcome.PaymentDateBeforeIssue, outcome);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Null(invoice.PaidAtUtc);
    }

    [Fact]
    public void MarkPaid_WhenAlreadyPaid_RejectsTransition()
    {
        var originalPaidAtUtc = IssuedAtUtc.AddDays(1);
        var invoice = CreateInvoice(InvoiceStatus.Paid, originalPaidAtUtc);

        var outcome = invoice.MarkPaid(originalPaidAtUtc.AddDays(1));

        Assert.Equal(MarkInvoicePaidOutcome.AlreadyPaid, outcome);
        Assert.Equal(originalPaidAtUtc, invoice.PaidAtUtc);
    }

    [Fact]
    public void CheckRefundEligibility_WhenPaidWithinFourteenDays_ReturnsEligible()
    {
        var paidAtUtc = IssuedAtUtc.AddDays(1);
        var invoice = CreateInvoice(InvoiceStatus.Paid, paidAtUtc);

        var eligibility = invoice.CheckRefundEligibility(paidAtUtc.AddDays(13));

        Assert.Equal(RefundEligibility.Eligible, eligibility);
    }

    [Fact]
    public void CheckRefundEligibility_WhenUnpaid_ReturnsInvoiceNotPaid()
    {
        var invoice = CreateInvoice();

        var eligibility = invoice.CheckRefundEligibility(IssuedAtUtc.AddDays(1));

        Assert.Equal(RefundEligibility.InvoiceNotPaid, eligibility);
    }

    [Fact]
    public void CheckRefundEligibility_WhenAlreadyRefunded_ReturnsAlreadyRefunded()
    {
        var paidAtUtc = IssuedAtUtc.AddDays(1);
        var invoice = CreateInvoice(InvoiceStatus.Refunded, paidAtUtc);

        var eligibility = invoice.CheckRefundEligibility(paidAtUtc.AddDays(2));

        Assert.Equal(RefundEligibility.AlreadyRefunded, eligibility);
    }

    [Fact]
    public void CheckRefundEligibility_WhenOutsideFourteenDays_ReturnsWindowExpired()
    {
        var paidAtUtc = IssuedAtUtc.AddDays(1);
        var invoice = CreateInvoice(InvoiceStatus.Paid, paidAtUtc);

        var eligibility = invoice.CheckRefundEligibility(paidAtUtc.AddDays(15));

        Assert.Equal(RefundEligibility.RefundWindowExpired, eligibility);
    }

    private static Invoice CreateInvoice(
        InvoiceStatus status = InvoiceStatus.Unpaid,
        DateTimeOffset? paidAtUtc = null) =>
        Invoice.Create(
            InvoiceId,
            CustomerId,
            "INV-1001",
            125.50m,
            IssuedAtUtc,
            status,
            paidAtUtc);
}
