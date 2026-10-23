namespace CustomerSupport.Application.Agents;

public interface ICustomerSupportAgent
{
    Task<AgentResponse> RespondAsync(string message, CancellationToken cancellationToken = default);
}
