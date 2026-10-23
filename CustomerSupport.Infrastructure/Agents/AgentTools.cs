#pragma warning disable OPENAI001 // Responses API is marked experimental by the official SDK.
using OpenAI.Responses;

namespace CustomerSupport.Infrastructure.Agents;

internal static class AgentTools
{
    public static IEnumerable<ResponseTool> Create() =>
    [
        Function("get_subscription", "Get the authenticated customer's subscription.",
            """{"type":"object","properties":{},"required":[],"additionalProperties":false}"""),
        Function("get_invoices", "List the authenticated customer's invoices, newest first.",
            """{"type":"object","properties":{},"required":[],"additionalProperties":false}"""),
        Function("check_refund_eligibility", "Check refund eligibility for an owned invoice. Does not issue a refund.",
            """{"type":"object","properties":{"invoiceId":{"type":"string","format":"uuid"}},"required":["invoiceId"],"additionalProperties":false}"""),
        Function("create_support_ticket", "Create one ticket only after a direct customer request. Supply the customer's issue accurately.",
            """{"type":"object","properties":{"subject":{"type":"string","minLength":1,"maxLength":200},"description":{"type":"string","minLength":1,"maxLength":4000}},"required":["subject","description"],"additionalProperties":false}"""),
        Function("escalate_to_human", "Escalate an owned ticket only after a direct customer request. Use a ticket ID provided by the customer or a successful creation tool result.",
            """{"type":"object","properties":{"ticketId":{"type":"string","format":"uuid"}},"required":["ticketId"],"additionalProperties":false}""")
    ];

    private static ResponseTool Function(string name, string description, string schema) =>
        ResponseTool.CreateFunctionTool(name, BinaryData.FromString(schema),
            strictModeEnabled: true, functionDescription: description);
}
