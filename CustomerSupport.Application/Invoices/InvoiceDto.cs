namespace CustomerSupport.Application.Invoices;

public sealed record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    decimal Amount,
    string Status,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset? PaidAtUtc);
