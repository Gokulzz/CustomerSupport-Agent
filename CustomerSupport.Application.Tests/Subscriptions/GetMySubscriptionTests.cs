using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Subscriptions;
using Xunit;

namespace CustomerSupport.Application.Tests.Subscriptions;

public sealed class GetMySubscriptionTests
{
    private static readonly Guid AliceCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BobCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task ExecuteAsync_WhenSubscriptionExists_ReturnsSubscription()
    {
        var expected = new SubscriptionDto(
            Guid.Parse("30000000-0000-0000-0000-000000000001"),
            "Pro",
            "Active",
            new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero),
            IsActive: true);
        var query = new StubSubscriptionQuery(expected);
        var useCase = new GetMySubscription(
            new StubCurrentCustomer(AliceCustomerId),
            query);

        var result = await useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSubscriptionDoesNotExist_ReturnsNotFoundFailure()
    {
        var useCase = new GetMySubscription(
            new StubCurrentCustomer(AliceCustomerId),
            new StubSubscriptionQuery(response: null));

        var result = await useCase.ExecuteAsync();

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription.NotFound", result.Error.Code);
        Assert.Equal(SubscriptionErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_QueriesOnlyTheCurrentCustomersId()
    {
        var query = new StubSubscriptionQuery(response: null);
        var useCase = new GetMySubscription(
            new StubCurrentCustomer(AliceCustomerId),
            query);

        await useCase.ExecuteAsync();

        Assert.Equal(AliceCustomerId, query.RequestedCustomerId);
        Assert.NotEqual(BobCustomerId, query.RequestedCustomerId);
    }

    private sealed class StubCurrentCustomer(Guid customerId) : ICurrentCustomer
    {
        public Guid CustomerId { get; } = customerId;
    }

    private sealed class StubSubscriptionQuery(SubscriptionDto? response) : ISubscriptionQuery
    {
        public Guid? RequestedCustomerId { get; private set; }

        public Task<SubscriptionDto?> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            RequestedCustomerId = customerId;

            return Task.FromResult(response);
        }
    }
}
