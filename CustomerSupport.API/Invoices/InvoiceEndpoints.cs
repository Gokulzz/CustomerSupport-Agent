using CustomerSupport.Application.Invoices;

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

        return endpoints;
    }

    private static async Task<IResult> GetMyInvoicesAsync(
        GetMyInvoices getMyInvoices,
        CancellationToken cancellationToken) =>
        Results.Ok(await getMyInvoices.ExecuteAsync(cancellationToken));
}
