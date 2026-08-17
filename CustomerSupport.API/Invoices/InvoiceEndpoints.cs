using CustomerSupport.Application.Invoices;
using CustomerSupport.Application.Results;

namespace CustomerSupport.Api.Invoices;

internal static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/invoices/me", GetMyInvoicesAsync)
            .RequireAuthorization()
            .WithTags("Invoices")
            .Produces<IReadOnlyList<InvoiceDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet(
                "/api/invoices/{invoiceId:guid}/refund-eligibility",
                CheckRefundEligibilityAsync)
            .RequireAuthorization()
            .WithTags("Invoices")
            .Produces<RefundEligibilityDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> GetMyInvoicesAsync(
        GetMyInvoices getMyInvoices,
        CancellationToken cancellationToken) =>
        Results.Ok(await getMyInvoices.ExecuteAsync(cancellationToken));

    private static async Task<IResult> CheckRefundEligibilityAsync(
        Guid invoiceId,
        CheckRefundEligibility checkRefundEligibility,
        CancellationToken cancellationToken)
    {
        var result = await checkRefundEligibility.ExecuteAsync(invoiceId, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapFailure(result.Error);
    }

    private static IResult MapFailure(Error error) =>
        error == InvoiceErrors.NotFound
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Invoice not found.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code
                })
            : throw new InvalidOperationException(
                $"Unhandled invoice error code '{error.Code}'.");
}
