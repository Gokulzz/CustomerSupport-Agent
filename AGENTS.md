# AGENTS.md

## Project

Production-style AI customer support system built incrementally with:

* .NET 10
* ASP.NET Core Minimal APIs
* Pragmatic Clean Architecture
* Modular monolith
* SQL Server + EF Core
* OpenAI
* Qdrant later for RAG
* xUnit

Build only the current requested milestone. Do not implement future features early.

## Architecture

Projects:

* `CustomerSupport.Domain`
* `CustomerSupport.Application`
* `CustomerSupport.Infrastructure`
* `CustomerSupport.Api`

Dependency rules:

* Domain depends on nothing.
* Application depends on Domain.
* Infrastructure depends on Application and Domain.
* Api is the composition root and depends on Application and Infrastructure.

Keep Minimal API endpoints thin.

Keep business rules in Domain/Application.

Keep EF Core, OpenAI, Qdrant, and other integrations in Infrastructure.

Prefer feature-based organization.

## Design Rules

Do not introduce unless explicitly requested:

* Microservices
* Multiple AI agents
* Semantic Kernel
* MediatR
* Generic repositories
* Event sourcing
* Distributed messaging
* Unnecessary abstractions

Use the simplest design that preserves clear boundaries.

## Error Handling

Expected business failures use the Result pattern.

Do not throw exceptions for normal business outcomes such as:

* Not found
* Not eligible
* Invalid business state
* Duplicate operation

Unexpected failures use exceptions.

Use:

* `IExceptionHandler`
* `ProblemDetails`
* Structured logging

Never expose stack traces or internal exception details to clients.

## Security

* Derive customer identity from authenticated server-side context.
* Never trust a customer ID supplied by the AI model.
* Validate all external input.
* Enforce authorization and resource ownership in application code.
* Apply rate limiting to appropriate endpoints.
* Use idempotency for state-changing operations where required.
* Never log passwords, tokens, API keys, or payment credentials.

## AI Agent

The initial system has one customer-support agent.

Before modifying agent behavior, tools, guardrails, or permissions, read:

`docs/AGENT_BOUNDARIES.md`

The model may select approved tools, but deterministic C# code must enforce:

* Authentication
* Authorization
* Ownership
* Business rules
* Sensitive-operation approval

Never expose unrestricted tools such as:

* `ExecuteSql`
* `ExecuteCommand`
* `CallAnyUrl`
* `AccessAnyCustomer`

## Coding

* Nullable reference types enabled.
* Use async APIs for I/O.
* Propagate `CancellationToken`.
* Prefer `DateTimeOffset` for persisted timestamps.
* Use UTC server timestamps.
* Prefer sealed classes when inheritance is unnecessary.
* Prefer records for immutable contracts.
* Use constructor injection.
* Avoid unrelated refactoring.

## Testing

Add tests for behavior introduced by each task.

Use:

* Domain tests for business rules.
* Application tests for use cases.
* Integration tests for API, EF Core, authentication, and tools.
* Evaluation tests later for AI behavior.

Before completing a substantial change:

```bash
dotnet build
dotnet test
```

Report anything that could not be verified.

## Workflow

For every Codex task:

1. Inspect the existing implementation.
2. Make the smallest change that satisfies the request.
3. Preserve architecture boundaries.
4. Add relevant tests.
5. Build and test.
6. Summarize changes and remaining limitations.
