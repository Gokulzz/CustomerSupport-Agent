using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Invoices;
using CustomerSupport.Domain.Invoices;
using Xunit;

namespace CustomerSupport.Application.Tests.Invoices;

public sealed class CheckRefundEligibilityTests
{
    private static readonly Guid AliceCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BobCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid AliceInvoiceId =
        Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid BobInvoiceId =
        Guid.Parse("40000000-0000-0000-0000-000000000004");
    private static readonly DateTimeOffset ReferenceTime =
        new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_WhenPaidWithinRefundWindow_ReturnsEligible()
    {
        var invoice = CreateInvoice(
            AliceInvoiceId,
            AliceCustomerId,
            InvoiceStatus.Paid,
            ReferenceTime.AddDays(-10));
        var useCase = CreateUseCase(invoice);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsSuccess);
        Assert.Equal(AliceInvoiceId, result.Value.InvoiceId);
        Assert.True(result.Value.IsEligible);
        Assert.Equal("Eligible", result.Value.Reason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaidOutsideRefundWindow_ReturnsRefundWindowExpired()
    {
        var invoice = CreateInvoice(
            AliceInvoiceId,
            AliceCustomerId,
            InvoiceStatus.Paid,
            ReferenceTime.AddDays(-15));
        var useCase = CreateUseCase(invoice);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEligible);
        Assert.Equal("RefundWindowExpired", result.Value.Reason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsUnpaid_ReturnsInvoiceNotPaid()
    {
        var invoice = CreateInvoice(
            AliceInvoiceId,
            AliceCustomerId,
            InvoiceStatus.Unpaid);
        var useCase = CreateUseCase(invoice);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEligible);
        Assert.Equal("InvoiceNotPaid", result.Value.Reason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsAlreadyRefunded_ReturnsAlreadyRefunded()
    {
        var invoice = CreateInvoice(
            AliceInvoiceId,
            AliceCustomerId,
            InvoiceStatus.Refunded,
            ReferenceTime.AddDays(-5));
        var useCase = CreateUseCase(invoice);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEligible);
        Assert.Equal("AlreadyRefunded", result.Value.Reason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoiceIsMissing_ReturnsInvoiceNotFound()
    {
        var useCase = CreateUseCase(invoice: null);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsFailure);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
        Assert.Equal("Invoice.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task ExecuteAsync_QueriesByRequestedInvoiceAndCurrentCustomer()
    {
        var query = new StubInvoiceQuery(response: null);
        var useCase = new CheckRefundEligibility(
            new StubCurrentCustomer(AliceCustomerId),
            query,
            new FixedTimeProvider(ReferenceTime));

        await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.Equal(AliceInvoiceId, query.RequestedInvoiceId);
        Assert.Equal(AliceCustomerId, query.RequestedCustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAliceRequestsBobsInvoice_ReturnsSameNotFoundOutcome()
    {
        var bobsInvoice = CreateInvoice(
            BobInvoiceId,
            BobCustomerId,
            InvoiceStatus.Paid,
            ReferenceTime.AddDays(-5));
        var query = new StubInvoiceQuery(bobsInvoice, enforceOwnership: true);
        var useCase = new CheckRefundEligibility(
            new StubCurrentCustomer(AliceCustomerId),
            query,
            new FixedTimeProvider(ReferenceTime));

        var result = await useCase.ExecuteAsync(BobInvoiceId);

        Assert.True(result.IsFailure);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
        Assert.Equal(BobInvoiceId, query.RequestedInvoiceId);
        Assert.Equal(AliceCustomerId, query.RequestedCustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_PassesTimeProviderUtcNowToDomainBehavior()
    {
        var invoice = CreateInvoice(
            AliceInvoiceId,
            AliceCustomerId,
            InvoiceStatus.Paid,
            ReferenceTime.AddDays(1));
        var timeProvider = new FixedTimeProvider(ReferenceTime);
        var useCase = new CheckRefundEligibility(
            new StubCurrentCustomer(AliceCustomerId),
            new StubInvoiceQuery(invoice),
            timeProvider);

        var result = await useCase.ExecuteAsync(AliceInvoiceId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEligible);
        Assert.Equal("PaymentDateInFuture", result.Value.Reason);
        Assert.Equal(1, timeProvider.GetUtcNowCallCount);
    }

    private static CheckRefundEligibility CreateUseCase(Invoice? invoice) =>
        new(
            new StubCurrentCustomer(AliceCustomerId),
            new StubInvoiceQuery(invoice),
            new FixedTimeProvider(ReferenceTime));

    private static Invoice CreateInvoice(
        Guid invoiceId,
        Guid customerId,
        InvoiceStatus status,
        DateTimeOffset? paidAtUtc = null)
    {
        var issuedAtUtc = paidAtUtc?.AddDays(-1) ?? ReferenceTime.AddDays(-30);

        return Invoice.Create(
            invoiceId,
            customerId,
            $"INV-{invoiceId:N}",
            49.00m,
            issuedAtUtc,
            status,
            paidAtUtc);
    }

    private sealed class StubCurrentCustomer(Guid customerId) : ICurrentCustomer
    {
        public Guid CustomerId { get; } = customerId;
    }

    private sealed class StubInvoiceQuery(
        Invoice? response,
        bool enforceOwnership = false) : IInvoiceQuery
    {
        public Guid? RequestedInvoiceId { get; private set; }

        public Guid? RequestedCustomerId { get; private set; }

        public Task<IReadOnlyList<InvoiceDto>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InvoiceDto>>([]);

        public Task<Invoice?> GetByIdAndCustomerIdAsync(
            Guid invoiceId,
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            RequestedInvoiceId = invoiceId;
            RequestedCustomerId = customerId;

            var matchesOwnership = response is not null
                && response.Id == invoiceId
                && response.CustomerId == customerId;

            return Task.FromResult(
                enforceOwnership && !matchesOwnership
                    ? null
                    : response);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public int GetUtcNowCallCount { get; private set; }

        public override DateTimeOffset GetUtcNow()
        {
            GetUtcNowCallCount++;
            return utcNow;
        }
    }
}
