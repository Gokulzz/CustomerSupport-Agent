namespace CustomerSupport.Infrastructure.Agents;

// This state is created for one RespondAsync invocation, never shared across requests.
internal sealed class AgentTurn
{
    public Guid? CreatedTicketId { get; set; }
    public Guid? EscalatedTicketId { get; set; }

    public string FallbackMessage()
    {
        var completed = CreatedTicketId is { } created ? $" Ticket {created} was created." : "";
        if (EscalatedTicketId is { } escalated)
            completed += $" Ticket {escalated} was escalated to human support.";

        return "I couldn't complete the full request. Please try a smaller request." + completed;
    }
}
