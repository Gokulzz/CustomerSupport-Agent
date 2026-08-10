# Business Scope

## Product

This project uses a fictional SaaS product named **CloudDesk**.

CloudDesk is a subscription-based team productivity platform.

The system is intentionally fictional so that customer data, business rules, support documentation, and expected outcomes can be seeded and tested deterministically.

---

## Version 1 Support Scope

The initial customer-support agent supports only these workflows:

### Subscription Support

The customer can ask:

* What subscription plan am I on?
* Is my subscription active?
* When does my subscription renew?
* What is my current plan status?

### Invoice and Billing Support

The customer can ask:

* Show my invoices.
* Show my latest invoice.
* Was a specific invoice paid?
* Why is an invoice still unpaid?

### Refund Eligibility

The customer can ask whether a specific invoice is eligible for a refund.

Initial refund rules:

* The invoice must exist.
* The invoice must belong to the authenticated customer.
* The invoice must be paid.
* The invoice must not already have been refunded.
* The invoice must have been paid within the last 14 days.

Version 1 only checks refund eligibility.

The agent does not issue refunds.

### Product and Troubleshooting Support

The customer can ask product questions that can be answered using the approved CloudDesk knowledge base.

Examples:

* How do I add a team member?
* What are the plan limits?
* How do I export a project?
* How do I resolve error E101?
* How do I resolve error E102?

These answers must be grounded in approved support documentation.

### Support Tickets

The customer can request that a support ticket be created for an unresolved issue.

Ticket creation is a controlled state-changing operation.

The system must validate the request before creating the ticket.

### Human Escalation

The conversation may be escalated when:

* The customer explicitly requests human support.
* The knowledge base does not contain enough information.
* A required operation is not supported.
* A tool repeatedly fails.
* The agent cannot safely determine the correct next action.

---

## Out of Scope for Version 1

Do not implement these features yet:

* Multiple AI agents
* Multi-tenancy
* Actual refund execution
* Subscription cancellation or modification
* Payment-method changes
* Account credits
* Arbitrary customer data access
* Arbitrary SQL execution
* External CRM integration
* External payment-provider integration
* Voice support
* Email support
* Long-term AI personalization
* Advanced analytics
* Recommendation systems
* Or Any other features not explicitly required by a concrete workflow.

These may be introduced later only when explicitly required.

---

## Core Business Entities

Version 1 is expected to use these core business concepts:

* Customer
* Subscription
* Plan
* Invoice
* SupportTicket
* Conversation
* ConversationMessage

Additional entities should be introduced only when required by a concrete workflow.

---

## Plans

CloudDesk initially has three plans:

### Free

* Basic product access
* Limited team size

### Pro

* Larger team allowance
* Additional product features

### Business

* Highest team allowance
* Advanced product features

Exact limits and pricing may be defined later when required by a support workflow.

---

## Deterministic Seed Scenarios

Development and automated tests should use fictional deterministic data.

Seed values should be stable enough for tests to reference known scenarios.

### Customer Alice

Alice represents a normal active customer.

Expected data:

* Active Pro subscription
* Subscription renewal date in the future
* One recently paid invoice that is eligible for refund
* One older paid invoice that is outside the refund window
* One existing open support ticket

Example scenarios:

* Alice asks for her current subscription.
* Alice asks for her latest invoices.
* Alice checks a refundable invoice.
* Alice checks an invoice outside the refund window.
* Alice creates a support ticket.

### Customer Bob

Bob is used primarily for authorization and edge-case testing.

Expected data:

* Cancelled or inactive subscription
* At least one invoice
* At least one failed or unpaid billing scenario

Alice must never be able to retrieve Bob's customer-specific data.

Example security scenario:

Alice asks for Bob's invoice number.

Expected result:

* Bob's invoice details are not returned.
* No sensitive information is exposed.

### Customer Charlie

Charlie represents a customer with minimal billing history.

Expected data:

* Expired or inactive trial
* No paid invoices
* No existing support tickets

Example scenarios:

* Charlie asks for invoices and receives an empty result.
* Charlie asks about subscription status.

---

## Invoice Seed Cases

Seed data should include at least these invoice states:

* Paid within the last 14 days
* Paid more than 14 days ago
* Already refunded
* Unpaid
* Failed payment
* Invoice belonging to another customer

These cases exist so refund and authorization behavior can be tested deterministically.

---

## Knowledge Base

The initial CloudDesk knowledge base should eventually include documents such as:

* `subscription-plans.md`
* `billing-and-invoices.md`
* `refund-policy.md`
* `team-member-limits.md`
* `project-export.md`
* `error-e101.md`
* `error-e102.md`
* `support-escalation.md`

Knowledge documents should contain explicit facts that can be verified during agent evaluation.

---

## Expected Agent Capabilities

The initial AI agent may eventually use approved operations such as:

* `SearchKnowledge`
* `GetSubscription`
* `GetInvoices`
* `CheckRefundEligibility`
* `CreateSupportTicket`
* `EscalateToHuman`

The agent may select an approved operation, but it does not control authorization or business policy.

---

## Business Source of Truth

The following information must come from deterministic application code or authoritative storage:

* Customer identity
* Subscription status
* Invoice ownership
* Invoice payment status
* Refund eligibility
* Ticket state
* Business rules

The AI model must not invent or override this information.

---

## Guiding Principle

The goal of this project is not to build a generic chatbot.

The goal is to build a secure and testable customer-support system where the AI understands the customer's request and selects an approved operation, while deterministic C# application code validates, authorizes, executes, and records the result.
