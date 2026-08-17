using CustomerSupport.Application.SupportTickets;

namespace CustomerSupport.Api.SupportTickets;

internal static class SupportTicketEndpoints
{
    public static IEndpointRouteBuilder MapSupportTicketEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/support-tickets", CreateSupportTicketAsync)
            .RequireAuthorization()
            .WithTags("Support Tickets")
            .Produces<SupportTicketDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> CreateSupportTicketAsync(
        CreateSupportTicketRequest request,
        CreateSupportTicket createSupportTicket,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            errors["subject"] = ["Subject is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors["description"] = ["Description is required."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var response = await createSupportTicket.ExecuteAsync(request, cancellationToken);

        return Results.Json(response, statusCode: StatusCodes.Status201Created);
    }
}
