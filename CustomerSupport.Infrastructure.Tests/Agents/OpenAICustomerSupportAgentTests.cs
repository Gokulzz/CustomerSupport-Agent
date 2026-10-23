#pragma warning disable OPENAI001 // Exercise the official Responses SDK with an in-memory HTTP transport.
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using CustomerSupport.Domain.SupportTickets;
using CustomerSupport.Infrastructure.Agents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OpenAI.Responses;
using Xunit;

namespace CustomerSupport.Infrastructure.Tests.Agents;

public sealed class OpenAICustomerSupportAgentTests
{
    [Fact]
    public async Task SequentialToolsReturnCallOutputsWithPreviousResponseContext()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(index => index switch
        {
            0 => Response("resp_1", Call("call_1", "get_invoices")),
            1 => Response("resp_2", Call("call_2", "get_subscription")),
            _ => Response("resp_3", Text("You have no invoices or subscription."))
        });
        var agent = CreateAgent(fixture, transport);

        var result = await agent.RespondAsync("My invoices and plan?");

        Assert.Equal("You have no invoices or subscription.", result.Message);
        Assert.Equal(3, transport.Requests.Count);
        Assert.Equal(2, fixture.Reads);
        var first = transport.Requests[0];
        Assert.Equal("test-model", first.GetProperty("model").GetString());
        Assert.False(first.GetProperty("parallel_tool_calls").GetBoolean());
        var second = transport.Requests[1];
        Assert.Equal("resp_1", second.GetProperty("previous_response_id").GetString());
        var output = second.GetProperty("input")[0];
        Assert.Equal("function_call_output", output.GetProperty("type").GetString());
        Assert.Equal("call_1", output.GetProperty("call_id").GetString());
        Assert.Contains("\"success\":true", output.GetProperty("output").GetString());
        var third = transport.Requests[2];
        Assert.Equal("resp_2", third.GetProperty("previous_response_id").GetString());
        Assert.Equal("call_2", third.GetProperty("input")[0].GetProperty("call_id").GetString());
        Assert.Contains("Subscription.NotFound", third.GetProperty("input")[0].GetProperty("output").GetString());
        Assert.All(transport.Requests, request => Assert.False(string.IsNullOrWhiteSpace(request.GetProperty("instructions").GetString())));
    }

    [Fact]
    public async Task EndlessToolRequestsStopAtFiveCallsAndSixModelRequests()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(index => Response($"resp_{index}", Call($"call_{index}", "get_invoices")));
        var logger = new RecordingLogger();
        var result = await CreateAgent(fixture, transport, logger).RespondAsync("Show invoices");
        Assert.Equal(5, fixture.Reads);
        Assert.Equal(6, transport.Requests.Count);
        Assert.Contains("couldn't complete", result.Message);
        Assert.Contains(logger.Messages, entry => entry.Contains("iteration limit reached"));
    }

    [Fact]
    public async Task OversizedToolBatchCannotBypassCallBudget()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(_ => Response("resp_1",
            Enumerable.Range(0, 6).Select(index => Call($"call_{index}", "get_invoices")).ToArray()));
        var result = await CreateAgent(fixture, transport).RespondAsync("Show invoices");
        Assert.Equal(0, fixture.Reads);
        Assert.Single(transport.Requests);
        Assert.Contains("couldn't complete", result.Message);
    }

    [Fact]
    public async Task EscalationDoesNotRequireTicketIdInUserMessage()
    {
        var fixture = new AgentFixture();
        fixture.Ticket = SupportTicket.Create(Guid.NewGuid(), fixture.CustomerId, "Export", "Fails", AgentFixture.Now);
        using var transport = new FakeOpenAIHandler(index => index == 0
            ? Response("resp_1", Call("call_1", "escalate_to_human", JsonSerializer.Serialize(new { ticketId = fixture.Ticket.Id })))
            : Response("resp_2", Text("Your ticket was escalated.")));
        await CreateAgent(fixture, transport).RespondAsync("Please escalate this to a human.");
        Assert.Equal(1, fixture.Saves);
        Assert.Equal(TicketStatus.Escalated, fixture.Ticket.Status);
        Assert.Contains("\"success\":true", transport.Requests[1].GetProperty("input")[0].GetProperty("output").GetString());
    }

    [Fact]
    public async Task FallbackAcknowledgesSuccessfulWritesWithoutRepeatingThem()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(index => Response($"resp_{index}",
            Call($"call_{index}", "create_support_ticket", """{"subject":"Export","description":"Fails"}""")));
        var result = await CreateAgent(fixture, transport).RespondAsync("Please open a ticket. Export fails.");
        Assert.Equal(1, fixture.Creates);
        Assert.Contains(fixture.Ticket!.Id.ToString(), result.Message);
        Assert.Contains("was created", result.Message);
    }

    [Fact]
    public async Task EachRequestStartsWithoutPreviousConversationState()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(index => Response($"resp_{index}", Text("Hello")));
        var agent = CreateAgent(fixture, transport);
        await agent.RespondAsync("Hello");
        await agent.RespondAsync("Hello again");
        Assert.All(transport.Requests, request =>
            Assert.True(!request.TryGetProperty("previous_response_id", out var previous) || previous.ValueKind == JsonValueKind.Null));
    }

    [Fact]
    public async Task SchemasExposeOnlyFiveApprovedToolsAndMinimalStrictArguments()
    {
        using var transport = new FakeOpenAIHandler(_ => Response("resp_1", Text("Hello")));
        await CreateAgent(new AgentFixture(), transport).RespondAsync("Hello");
        var expected = new Dictionary<string, string[]>
        {
            ["get_subscription"] = [], ["get_invoices"] = [],
            ["check_refund_eligibility"] = ["invoiceId"],
            ["create_support_ticket"] = ["description", "subject"],
            ["escalate_to_human"] = ["ticketId"]
        };
        var tools = transport.Requests.Single().GetProperty("tools");
        Assert.Equal(expected.Count, tools.GetArrayLength());
        foreach (var tool in tools.EnumerateArray())
        {
            Assert.Equal("function", tool.GetProperty("type").GetString());
            Assert.True(tool.GetProperty("strict").GetBoolean());
            var parameters = tool.GetProperty("parameters");
            Assert.False(parameters.GetProperty("additionalProperties").GetBoolean());
            var fields = parameters.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order().ToArray();
            Assert.Equal(expected[tool.GetProperty("name").GetString()!], fields);
            Assert.Equal(fields, parameters.GetProperty("required").EnumerateArray().Select(field => field.GetString()).Order().ToArray());
        }
    }

    [Fact]
    public async Task OpenAIFailureLogsSafeMetadataAndThrowsSanitizedException()
    {
        using var transport = new FakeOpenAIHandler(_ => """{"error":{"message":"sensitive remote error body","type":"invalid_request_error","code":"invalid_api_key"}}""", HttpStatusCode.Unauthorized);
        var logger = new RecordingLogger();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateAgent(new AgentFixture(), transport, logger).RespondAsync("private customer message"));
        Assert.Null(exception.InnerException);
        Assert.DoesNotContain("sensitive", exception.ToString());
        Assert.Contains(logger.Messages, entry => entry.Contains("OpenAI request failed"));
        Assert.DoesNotContain(logger.Messages, entry => entry.Contains("sensitive") || entry.Contains("private customer message") || entry.Contains("test-api-key"));
    }

    [Fact]
    public async Task CancelledRequestDoesNotStartOpenAICallOrToolExecution()
    {
        var fixture = new AgentFixture();
        using var transport = new FakeOpenAIHandler(_ => Response("resp_1", Call("call_1", "get_invoices")));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateAgent(fixture, transport).RespondAsync("Invoices", cancellation.Token));
        Assert.Empty(transport.Requests);
        Assert.Equal(0, fixture.Reads);
    }

    private static OpenAICustomerSupportAgent CreateAgent(
        AgentFixture fixture, FakeOpenAIHandler handler, ILogger<OpenAICustomerSupportAgent>? logger = null)
    {
        var client = new ResponsesClient(new ApiKeyCredential("test-api-key"), new ResponsesClientOptions
        {
            Transport = new HttpClientPipelineTransport(new HttpClient(handler)),
            RetryPolicy = new ClientRetryPolicy(0)
        });
        return new OpenAICustomerSupportAgent(client, fixture.Dispatcher(),
            Options.Create(new OpenAIOptions { Model = "test-model", ApiKey = "test-api-key" }),
            logger ?? NullLogger<OpenAICustomerSupportAgent>.Instance);
    }

    private static object Call(string id, string name, string args = "{}") =>
        new { type = "function_call", id = $"fc_{id}", call_id = id, name, arguments = args, status = "completed" };

    private static object Text(string text) => new
    {
        type = "message", id = "msg_1", status = "completed", role = "assistant",
        content = new[] { new { type = "output_text", text, annotations = Array.Empty<object>() } }
    };

    private static string Response(string id, params object[] output) => JsonSerializer.Serialize(new
    {
        id, @object = "response", created_at = 1789905600, status = "completed", model = "test-model", output
    });

    private sealed class FakeOpenAIHandler(Func<int, string> response, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public List<JsonElement> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("/v1/responses", request.RequestUri!.AbsolutePath);
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Requests.Add(json.RootElement.Clone());
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(response(Requests.Count - 1), Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class RecordingLogger : ILogger<OpenAICustomerSupportAgent>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
