namespace CustomerSupport.Application.Agents;

public sealed record AgentResponse(string Message, Guid? ConversationId = null);
