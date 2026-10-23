using System.Text.Json;
using CustomerSupport.Application.Invoices;
using CustomerSupport.Application.Subscriptions;
using CustomerSupport.Application.SupportTickets;

namespace CustomerSupport.Infrastructure.Agents;

internal sealed class AgentToolDispatcher(
    GetMySubscription getSubscription,
    GetMyInvoices getInvoices,
    CheckRefundEligibility checkRefundEligibility,
    CreateSupportTicket createTicket,
    EscalateSupportTicket escalateTicket)
{
    public static bool IsApproved(string name) => name is
        "get_subscription" or "get_invoices" or "check_refund_eligibility" or
        "create_support_ticket" or "escalate_to_human";

    public async Task<ToolResult> ExecuteAsync(
        string name, string arguments, AgentTurn turn, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsApproved(name))
            return ToolResult.Fail("Tool.Unknown", "This tool is not available.");

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(arguments);
        }
        catch (JsonException)
        {
            return InvalidArguments();
        }

        using (document)
        {
            var args = document.RootElement;
            switch (name)
            {
                case "get_subscription":
                    if (!HasExactly(args)) return InvalidArguments();
                    var subscription = await getSubscription.ExecuteAsync(cancellationToken);
                    return subscription.IsFailure ? ToolResult.Fail(subscription.Error) : ToolResult.Ok(new
                    {
                        plan = subscription.Value.PlanName,
                        subscription.Value.Status,
                        subscription.Value.RenewsAtUtc
                    });

                case "get_invoices":
                    if (!HasExactly(args)) return InvalidArguments();
                    return ToolResult.Ok(await getInvoices.ExecuteAsync(cancellationToken));

                case "check_refund_eligibility":
                    if (!HasExactly(args, "invoiceId") || !TryId(args, "invoiceId", out var invoiceId))
                        return InvalidArguments();
                    var refund = await checkRefundEligibility.ExecuteAsync(invoiceId, cancellationToken);
                    return refund.IsFailure ? ToolResult.Fail(refund.Error) : ToolResult.Ok(refund.Value);

                case "create_support_ticket":
                    if (!HasExactly(args, "subject", "description") ||
                        !TryText(args, "subject", 200, out var subject) ||
                        !TryText(args, "description", 4000, out var description))
                        return InvalidArguments();
                    if (turn.CreatedTicketId is not null) return WriteAlreadyPerformed();
                    var ticket = await createTicket.ExecuteAsync(
                        new CreateSupportTicketRequest(subject, description), cancellationToken);
                    turn.CreatedTicketId = ticket.Id;
                    return ToolResult.Ok(new { ticketId = ticket.Id, ticket.Status, ticket.OpenedAtUtc });

                case "escalate_to_human":
                    if (!HasExactly(args, "ticketId") || !TryId(args, "ticketId", out var ticketId))
                        return InvalidArguments();
                    if (turn.EscalatedTicketId is not null) return WriteAlreadyPerformed();
                    var escalation = await escalateTicket.ExecuteAsync(ticketId, cancellationToken);
                    if (escalation.IsFailure) return ToolResult.Fail(escalation.Error);
                    turn.EscalatedTicketId = escalation.Value.Id;
                    return ToolResult.Ok(new
                    {
                        ticketId = escalation.Value.Id,
                        escalation.Value.Status,
                        escalation.Value.EscalatedAtUtc
                    });

                default:
                    return ToolResult.Fail("Tool.Unknown", "This tool is not available.");
            }
        }
    }

    private static bool HasExactly(JsonElement args, params string[] names) =>
        args.ValueKind == JsonValueKind.Object &&
        args.EnumerateObject().Count() == names.Length &&
        names.All(name => args.TryGetProperty(name, out _));

    private static bool TryId(JsonElement args, string name, out Guid id)
    {
        id = default;
        return args.GetProperty(name).ValueKind == JsonValueKind.String &&
            Guid.TryParse(args.GetProperty(name).GetString(), out id) && id != Guid.Empty;
    }

    private static bool TryText(JsonElement args, string name, int maxLength, out string text)
    {
        text = args.GetProperty(name).ValueKind == JsonValueKind.String
            ? args.GetProperty(name).GetString() ?? "" : "";
        return !string.IsNullOrWhiteSpace(text) && text.Length <= maxLength;
    }

    private static ToolResult InvalidArguments() => ToolResult.Fail(
        "Tool.InvalidArguments", "The tool arguments are missing or invalid.");

    private static ToolResult WriteAlreadyPerformed() => ToolResult.Fail(
        "Tool.WriteAlreadyPerformed", "This write has already succeeded in this turn. Use the earlier tool result; do not repeat it.");
}
