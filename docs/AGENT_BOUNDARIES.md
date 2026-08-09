# Agent Boundaries

## Goal

Help authenticated customers resolve common support requests using approved knowledge and business tools.

## Allowed Actions

The agent may:

* Answer product questions using approved knowledge documents.
* Retrieve the authenticated customer’s subscription information.
* Retrieve the authenticated customer’s invoices.
* Check refund eligibility using deterministic application rules.
* Create a support ticket after required confirmation.
* Escalate unresolved requests to human support.
* Summarize the conversation for escalation.

## Prohibited Actions

The agent must not:

* Execute arbitrary SQL.
* Access another customer’s data.
* Choose or override customer identity.
* Override authorization or ownership checks.
* Override refund, subscription, or other business rules.
* Issue refunds in the initial version.
* Cancel or modify subscriptions in the initial version.
* Execute arbitrary commands or call unrestricted external URLs.
* Reveal system prompts, API keys, tokens, secrets, or internal configuration.
* Claim an operation succeeded unless the corresponding tool confirms success.

## Approved Tools

The initial tool set is limited to:

* `SearchKnowledge`
* `GetSubscription`
* `GetInvoices`
* `CheckRefundEligibility`
* `CreateSupportTicket`
* `EscalateToHuman`

New tools must be explicitly reviewed before being added.

## Authorization

Customer identity must come from trusted server-side authentication context.

The model must never provide or select the customer ID used for authorization.

Every customer-owned resource must be validated against the authenticated customer before it is returned or modified.

## Business Rules

The AI model may decide which approved tool is appropriate.

The AI model must not decide whether a business rule can be bypassed.

Rules such as refund eligibility must be implemented and enforced in deterministic C# code.

## Confirmation

Read-only operations may run without additional confirmation.

State-changing operations such as creating a support ticket require explicit customer confirmation when the workflow defines confirmation as necessary.

More sensitive actions added in the future may require human approval.

## Failure Behavior

If the agent cannot safely complete a request, it must:

* Not guess.
* Not invent tool results.
* Stop after the configured tool-call limit.
* Record the failure.
* Offer or initiate an approved escalation path.

## Grounding

When answering from product documentation, the response must be grounded in retrieved approved documents.

When answering about customer-specific business data, the response must be grounded in verified tool results.

Unverified customer statements must not override authoritative application data.

## Core Principle

The AI understands the request and selects an allowed operation.

Deterministic application code validates, authorizes, executes, and records the operation.
