# CloudDesk Customer Support Agent

A customer-support backend for **CloudDesk**, a fictional SaaS product. Built incrementally with .NET 10, ASP.NET Core Minimal APIs, EF Core + SQL Server, ASP.NET Core Identity/JWT, and the official OpenAI .NET SDK using the Responses API.

## What works today

- Customer login and access to their own subscription and invoices.
- Refund eligibility checks using the domain's 14-day payment window; no actual refunds.
- Support-ticket creation and escalation to human support.
- A single agent that calls existing Application use cases through five explicit tools: `get_subscription`, `get_invoices`, `check_refund_eligibility`, `create_support_ticket`, and `escalate_to_human`.
- Deterministic development data for Alice, Bob, and Charlie, plus Free, Pro, and Business plans.
- Structured logging with Serilog, expected failures using Result types, and global exception handling with ProblemDetails.

## Structure

The solution is a modular monolith following pragmatic Clean Architecture:

| Project / folder | Responsibility |
| --- | --- |
| `CustomerSupport.Domain` | Entities, invariants, and business behavior; no external dependencies |
| `CustomerSupport.Application` | Customer-scoped use cases, DTOs, Result types, and integration contracts |
| `CustomerSupport.Infrastructure` | EF Core mappings/migrations, Identity, seeding, and OpenAI tool orchestration |
| `CustomerSupport.API` | Minimal API endpoints, authentication context, and dependency-injection composition |

Customer identity comes from the validated JWT through `ICurrentCustomer`. Tools cannot supply `customerId`; resource ownership is enforced by Application use cases.

## Run locally

Prerequisites: **.NET 10 SDK**, a reachable **SQL Server** instance, and **dotnet-ef 10.x**. An OpenAI API key is needed for agent chat only.

Run these PowerShell commands from the repository root. Replace the placeholders with your local values; keep real secrets out of source control.

```powershell
dotnet restore
# Install once if dotnet-ef is not already available:
dotnet tool install --global dotnet-ef --version "10.*"

dotnet user-secrets set "ConnectionStrings:CustomerSupportDatabase" "<your SQL Server connection string>" --project CustomerSupport.API
dotnet user-secrets set "Jwt:SigningKey" "<your random signing key of at least 32 bytes>" --project CustomerSupport.API
dotnet user-secrets set "OpenAI:ApiKey" "<your OpenAI API key>" --project CustomerSupport.API

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project CustomerSupport.Infrastructure --startup-project CustomerSupport.API
dotnet dev-certs https --trust
dotnet run --project CustomerSupport.API --launch-profile https
```

`OpenAI:Model` is configurable and defaults to `gpt-5-mini`. Environment variables such as `OpenAI__ApiKey` can also supply configuration.

Migrations must be applied manually before starting the API. Development startup seeds missing records and users without duplicating them. Seed dates are fixed, so a seeded "recent" invoice does not stay within the refund window as time passes.

Open **https://localhost:7011/swagger**. Log in through `POST /api/auth/login`, then paste the returned `accessToken` into **Authorize** without the `Bearer` prefix.

Development-only accounts:

| Email | Password |
| --- | --- |
| `alice@clouddesk.test` | `AliceDev!2026` |
| `bob@clouddesk.test` | `BobDev!2026` |
| `charlie@clouddesk.test` | `CharlieDev!2026` |

## API

All routes except login require a JWT.

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/auth/login` | Log in with `email` and `password` |
| GET | `/api/subscriptions/me` | Get the current customer's subscription |
| GET | `/api/invoices/me` | List the current customer's invoices |
| GET | `/api/invoices/{invoiceId}/refund-eligibility` | Check an owned invoice's eligibility |
| POST | `/api/support-tickets` | Create a ticket with `subject` and `description` |
| POST | `/api/agent/chat` | Send `{ "message": "Show my invoices." }`; receive `{ "message": "..." }` |

Escalation is an internal Application capability exposed through the agent tool; it has no dedicated HTTP endpoint.

## Agent behavior and current limits

We use our own C# orchestration loop with the official OpenAI SDK, without an agent framework. The loop sends the customer's message and approved tool definitions to OpenAI, dispatches requested tools directly to existing Application use cases, and returns structured tool results to OpenAI until it produces a final answer or reaches the limit. It never calls this application's own HTTP API.

Each chat request allows at most **five tool rounds and five tool calls**, with one final model request to consume the last results. State is limited to the current request.

Only one ticket creation and one escalation can succeed per turn. Confirmation is currently prompt guidance, not a server-side approval gate; the dispatcher validates arguments and blocks duplicate successful writes. There is no cross-request idempotency.

RAG, Qdrant, persistent conversation history, long-term memory, reflection, and multi-agent orchestration are not implemented.

## Tests

```powershell
dotnet build
dotnet test
```

xUnit tests cover Domain rules, Application use cases, the tool dispatcher, and bounded orchestration. Agent tests use a fake HTTP transport with the real SDK and do not make live OpenAI calls or require SQL Server.

Further details: [Business scope](docs/BUSINESS_Scope.md), [Agent boundaries](docs/AGENT_BOUNDARIES.md), and [Agent setup](docs/AGENT_SETUP.md).
