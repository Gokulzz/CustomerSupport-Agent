namespace CustomerSupport.Application.Agents;

public interface ICustomerSupportAgent
{
    Task<AgentResponse> RespondAsync(string message, Guid? conversationId = null, CancellationToken cancellationToken = default);
}
