using System.Text.Json;
using CustomerSupport.Application.Invoices;
using CustomerSupport.Application.Subscriptions;
using CustomerSupport.Domain.Invoices;
using CustomerSupport.Domain.SupportTickets;
using CustomerSupport.Infrastructure.Agents;
using Xunit;

namespace CustomerSupport.Infrastructure.Tests.Agents;

public sealed class AgentToolDispatcherTests
{
    [Fact]
    public async Task SubscriptionCallsUseCaseWithCurrentCustomerAndReturnsMinimalDto()
    {
        var fixture = new AgentFixture { Subscription = new SubscriptionDto(Guid.NewGuid(), "Pro", "Active", AgentFixture.Now, true) };
        var result = await fixture.Dispatcher().ExecuteAsync("get_subscription", "{}", new(), default);
        Assert.True(result.Success);
        Assert.Equal(fixture.CustomerId, fixture.QueriedCustomerId);
        var data = JsonDocument.Parse(result.ToJson()).RootElement.GetProperty("data");
        Assert.Equal("Pro", data.GetProperty("plan").GetString());
        Assert.False(data.TryGetProperty("customerId", out _));
    }

    [Fact]
    public async Task InvoicesCallsExistingUseCaseAndPropagatesCancellation()
    {
        var fixture = new AgentFixture { Invoices = [new InvoiceDto(Guid.NewGuid(), "INV-1", 10, "Paid", AgentFixture.Now, AgentFixture.Now)] };
        using var source = new CancellationTokenSource();
        var result = await fixture.Dispatcher().ExecuteAsync("get_invoices", "{}", new(), source.Token);
        Assert.True(result.Success);
        Assert.Equal(fixture.CustomerId, fixture.QueriedCustomerId);
        Assert.Equal(source.Token, fixture.ReceivedCancellationToken);
        Assert.Contains("INV-1", result.ToJson());
    }

    [Fact]
    public async Task RefundPassesInvoiceIdAndUseCaseSuppliesAuthenticatedOwner()
    {
        var fixture = new AgentFixture();
        fixture.Invoice = Invoice.Create(Guid.NewGuid(), fixture.CustomerId, "INV-1", 10, AgentFixture.Now.AddDays(-2), InvoiceStatus.Paid, AgentFixture.Now.AddDays(-1));
        var result = await fixture.Dispatcher().ExecuteAsync("check_refund_eligibility", JsonSerializer.Serialize(new { invoiceId = fixture.Invoice.Id }), new(), default);
        Assert.True(result.Success);
        Assert.Equal(fixture.Invoice.Id, fixture.QueriedResourceId);
        Assert.Equal(fixture.CustomerId, fixture.QueriedCustomerId);
        Assert.Contains("Eligible", result.ToJson());
    }

    [Fact]
    public async Task AnotherCustomersInvoiceReturnsControlledNotFound()
    {
        var fixture = new AgentFixture();
        fixture.Invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), "BOBS-INVOICE", 10, AgentFixture.Now);
        var result = await fixture.Dispatcher().ExecuteAsync("check_refund_eligibility", JsonSerializer.Serialize(new { invoiceId = fixture.Invoice.Id }), new(), default);
        Assert.False(result.Success);
        Assert.Equal("Invoice.NotFound", result.ErrorCode);
        Assert.DoesNotContain("BOBS-INVOICE", result.ToJson());
    }

    [Fact]
    public async Task CreatePassesContentToUseCaseAndPreventsRepeatedWrites()
    {
        var fixture = new AgentFixture();
        var dispatcher = fixture.Dispatcher();
        var turn = new AgentTurn();
        var args = """{"subject":"Export","description":"Export fails."}""";
        var result = await dispatcher.ExecuteAsync("create_support_ticket", args, turn, default);
        var repeat = await dispatcher.ExecuteAsync("create_support_ticket", args, turn, default);
        Assert.True(result.Success);
        Assert.Equal("Export", fixture.Ticket!.Subject);
        Assert.Equal("Export fails.", fixture.Ticket.Description);
        Assert.Equal(fixture.CustomerId, fixture.Ticket.CustomerId);
        Assert.Equal(AgentFixture.Now, fixture.Ticket.OpenedAtUtc);
        Assert.Equal(1, fixture.Creates);
        Assert.Equal("Tool.WriteAlreadyPerformed", repeat.ErrorCode);
        Assert.DoesNotContain("description", result.ToJson());
    }

    [Fact]
    public async Task EscalationPassesTicketIdToUseCaseAndPreventsRepeatedWrites()
    {
        var fixture = new AgentFixture();
        fixture.Ticket = SupportTicket.Create(Guid.NewGuid(), fixture.CustomerId, "Export", "Fails", AgentFixture.Now.AddDays(-1));
        var dispatcher = fixture.Dispatcher();
        var turn = new AgentTurn();
        var args = JsonSerializer.Serialize(new { ticketId = fixture.Ticket.Id });
        var result = await dispatcher.ExecuteAsync("escalate_to_human", args, turn, default);
        var repeat = await dispatcher.ExecuteAsync("escalate_to_human", args, turn, default);
        Assert.True(result.Success);
        Assert.Equal(fixture.Ticket.Id, fixture.QueriedResourceId);
        Assert.Equal(fixture.CustomerId, fixture.QueriedCustomerId);
        Assert.Equal(TicketStatus.Escalated, fixture.Ticket.Status);
        Assert.Equal(1, fixture.Saves);
        Assert.Equal(1, fixture.Reads);
        Assert.Equal(fixture.Ticket.Id, turn.EscalatedTicketId);
        Assert.Equal("Tool.WriteAlreadyPerformed", repeat.ErrorCode);
    }

    [Fact]
    public async Task UnknownToolRejectedWithoutCallingUseCases()
    {
        var fixture = new AgentFixture();
        var result = await fixture.Dispatcher().ExecuteAsync("execute_sql", "{}", new(), default);
        Assert.Equal("Tool.Unknown", result.ErrorCode);
        Assert.Equal(0, fixture.Reads + fixture.Creates + fixture.Saves);
    }

    [Theory]
    [InlineData("get_invoices", "{\"customerId\":\"other\"}")]
    [InlineData("get_invoices", "[]")]
    [InlineData("check_refund_eligibility", "{\"invoiceId\":\"invalid\"}")]
    [InlineData("create_support_ticket", "{\"subject\":\"  \",\"description\":\"valid\"}")]
    [InlineData("create_support_ticket", "{\"subject\":\"a\",\"subject\":\"b\",\"description\":\"c\"}")]
    [InlineData("get_subscription", "broken json")]
    [InlineData("escalate_to_human", "{}")]
    [InlineData("escalate_to_human", "{\"ticketId\":\"invalid\"}")]
    [InlineData("escalate_to_human", "{\"ticketId\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("escalate_to_human", "{\"ticketId\":null}")]
    public async Task MalformedOrExtraArgumentsAreRejected(string tool, string args)
    {
        var fixture = new AgentFixture();
        var result = await fixture.Dispatcher().ExecuteAsync(tool, args, new(), default);
        Assert.Equal("Tool.InvalidArguments", result.ErrorCode);
        Assert.Equal("The tool arguments are missing or invalid.", result.Message);
        Assert.Equal(0, fixture.Reads + fixture.Creates + fixture.Saves);
    }

    [Fact]
    public async Task EscalationStillEnforcesOwnershipAndFailureDoesNotConsumeWrite()
    {
        var fixture = new AgentFixture();
        fixture.Ticket = SupportTicket.Create(Guid.NewGuid(), Guid.NewGuid(), "Other customer's ticket", "Fails", AgentFixture.Now);
        var args = JsonSerializer.Serialize(new { ticketId = fixture.Ticket.Id });
        var dispatcher = fixture.Dispatcher();
        var turn = new AgentTurn();
        var failure = await dispatcher.ExecuteAsync("escalate_to_human", args, turn, default);
        Assert.Equal("SupportTicket.NotFound", failure.ErrorCode);
        Assert.Equal(0, fixture.Saves);
        Assert.Null(turn.EscalatedTicketId);

        fixture.Ticket = SupportTicket.Create(Guid.NewGuid(), fixture.CustomerId, "My ticket", "Fails", AgentFixture.Now);
        var success = await dispatcher.ExecuteAsync("escalate_to_human",
            JsonSerializer.Serialize(new { ticketId = fixture.Ticket.Id }), turn, default);
        Assert.True(success.Success);
        Assert.Equal(1, fixture.Saves);
    }

    [Fact]
    public async Task ApplicationFailureIsASafeStructuredOutput()
    {
        var fixture = new AgentFixture();
        var result = await fixture.Dispatcher().ExecuteAsync("get_subscription", "{}", new(), default);
        var json = JsonDocument.Parse(result.ToJson()).RootElement;
        Assert.False(json.GetProperty("success").GetBoolean());
        Assert.Equal("Subscription.NotFound", json.GetProperty("errorCode").GetString());
        Assert.False(json.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task UnexpectedFailuresBubbleInsteadOfBecomingToolData()
    {
        var fixture = new AgentFixture { ReadFailure = new InvalidOperationException("Database unavailable") };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Dispatcher().ExecuteAsync("get_invoices", "{}", new(), default));
    }
}
