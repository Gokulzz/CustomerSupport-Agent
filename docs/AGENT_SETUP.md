# First CloudDesk agent

`POST /api/agent/chat` requires a JWT. Its only request field is `message`;
the response contains `message`. Empty messages return validation ProblemDetails (400).

## Configuration

The API project already has a UserSecretsId. Supply the key locally:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "<your-key>" --project CustomerSupport.API
```

`OpenAI__ApiKey` is the equivalent environment variable. Do not put a real key in
appsettings or source control. `OpenAI:Model` is configurable (default `gpt-5-mini`).
Typed `OpenAIOptions` binds both properties and validates them when the agent is resolved.
No live OpenAI call is needed for the unit tests.

## Tools and continuation

| Tool | Arguments |
| --- | --- |
| get_subscription | none (`{}`) |
| get_invoices | none (`{}`) |
| check_refund_eligibility | invoiceId |
| create_support_ticket | subject, description |
| escalate_to_human | ticketId |

The dispatcher validates arguments and invokes the existing Application use cases directly.
Those use cases derive ownership from ICurrentCustomer. There are no loopback HTTP calls,
repository access by the model, or model-selected customer identities.

The official OpenAI .NET SDK sends strict function schemas. Each continuation supplies
`previous_response_id` and `function_call_output` items with matching `call_id` values.
Instructions and tools are supplied on every request. Response IDs are held only within
one RespondAsync invocation; no history is accepted from the caller or stored locally.
This uses Responses API server-side response storage for continuation (the API default).

The loop allows at most five tool rounds and five individual calls total, even when a
response contains multiple calls. One final model request may consume the fifth round's
outputs; any additional tool request is stopped before execution. A fallback identifies
successful writes already completed during this turn.

## Per-turn write protection

AgentTurn holds only successful creation and escalation IDs and builds the fallback
message. It does not receive or parse the customer's message. The dispatcher validates
tool arguments and prevents duplicate successful writes; it does not implement a
confirmation gate. Confirmation remains guidance in the agent instructions.

Escalation requires a valid, non-empty ticketId tool argument, but that ID need not
appear in the original customer message. The existing Application use case continues
to enforce authenticated customer ownership. There is no cross-request history.

Only one creation and one escalation can succeed per turn. Repeated model write calls
are rejected. This prevents duplicate writes inside the tool loop; it does not provide
cross-request idempotency for a client resubmitting a chat request.

Logs contain event names, approved tool names, outcome codes, iteration counts and safe
OpenAI failure metadata. They do not include customer messages, tool arguments, API keys,
tool payloads or upstream error bodies.

The SDK's Responses types currently require the file-scoped `OPENAI001` diagnostic
suppression. API version changes should be reviewed when upgrading the pinned SDK.

References: [OpenAI function calling](https://developers.openai.com/api/docs/guides/function-calling)
and [official .NET quickstart](https://developers.openai.com/api/docs/quickstart).
