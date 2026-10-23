using CustomerSupport.Application.Agents;

namespace CustomerSupport.Api.Agents;

internal static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/agent/chat", RespondAsync)
            .RequireAuthorization()
            .WithTags("Agent")
            .Produces<AgentResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);
        return endpoints;
    }

    private static async Task<IResult> RespondAsync(
        AgentChatRequest request, ICustomerSupportAgent agent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["message"] = ["Message is required."]
            });

        return Results.Ok(await agent.RespondAsync(request.Message, cancellationToken));
    }
}

internal sealed record AgentChatRequest(string? Message);
