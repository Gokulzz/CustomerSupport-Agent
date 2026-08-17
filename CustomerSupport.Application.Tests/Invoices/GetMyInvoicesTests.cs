using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.Invoices;
using Xunit;

namespace CustomerSupport.Application.Tests.Invoices;

public sealed class GetMyInvoicesTests
{
    private static readonly Guid AliceCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BobCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task ExecuteAsync_UsesTheCurrentCustomersId()
    {
        var query = new StubInvoiceQuery([]);
        var useCase = new GetMyInvoices(
            new StubCurrentCustomer(AliceCustomerId),
            query);

        await useCase.ExecuteAsync();

        Assert.Equal(AliceCustomerId, query.RequestedCustomerId);
        Assert.NotEqual(BobCustomerId, query.RequestedCustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvoicesExist_ReturnsInvoices()
    {
        InvoiceDto[] expected =
        [
            new(
                Guid.Parse("40000000-0000-0000-0000-000000000001"),
                "INV-ALICE-001",
                49.00m,
                "Paid",
                new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 2, 12, 0, 0, TimeSpan.Zero))
        ];
        var useCase = new GetMyInvoices(
            new StubCurrentCustomer(AliceCustomerId),
            new StubInvoiceQuery(expected));

        var result = await useCase.ExecuteAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoInvoicesExist_ReturnsEmptyCollection()
    {
        var useCase = new GetMyInvoices(
            new StubCurrentCustomer(AliceCustomerId),
            new StubInvoiceQuery([]));

        var result = await useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    private sealed class StubCurrentCustomer(Guid customerId) : ICurrentCustomer
    {
        public Guid CustomerId { get; } = customerId;
    }

    private sealed class StubInvoiceQuery(IReadOnlyList<InvoiceDto> response) : IInvoiceQuery
    {
        public Guid? RequestedCustomerId { get; private set; }

        public Task<IReadOnlyList<InvoiceDto>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            RequestedCustomerId = customerId;

            return Task.FromResult(response);
        }
    }
}
