namespace CustomerSupport.Application.Invoices;

public sealed record RefundEligibilityDto(
    Guid InvoiceId,
    bool IsEligible,
    string Reason);
