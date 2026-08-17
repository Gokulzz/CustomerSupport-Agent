using CustomerSupport.Application.Results;
using CustomerSupport.Application.Subscriptions;

namespace CustomerSupport.Api.Subscriptions;

internal static class SubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/subscriptions/me", GetMySubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Subscriptions")
            .Produces<SubscriptionDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> GetMySubscriptionAsync(
        GetMySubscription mySubscription,
        CancellationToken cancellationToken)
    {
        var result = await mySubscription.ExecuteAsync(cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : MapFailure(result.Error);
    }

    private static IResult MapFailure(Error error) =>
        error == SubscriptionErrors.NotFound
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Subscription not found.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code
                })
            : throw new InvalidOperationException(
                $"Unhandled subscription error code '{error.Code}'.");
}
