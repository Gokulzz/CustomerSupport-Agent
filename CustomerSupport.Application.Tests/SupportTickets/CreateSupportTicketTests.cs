using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.SupportTickets;
using CustomerSupport.Domain.SupportTickets;
using Xunit;

namespace CustomerSupport.Application.Tests.SupportTickets;

public sealed class CreateSupportTicketTests
{
    private static readonly Guid AliceCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset OpenedAtUtc =
        new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_CreatesTicketForAuthenticatedCustomer()
    {
        var repository = new StubSupportTicketRepository();
        var useCase = CreateUseCase(repository);

        await useCase.ExecuteAsync(CreateRequest());

        Assert.NotNull(repository.AddedTicket);
        Assert.Equal(AliceCustomerId, repository.AddedTicket.CustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_PassesSubjectAndDescriptionToDomainFactory()
    {
        var repository = new StubSupportTicketRepository();
        var useCase = CreateUseCase(repository);
        var request = CreateRequest();

        await useCase.ExecuteAsync(request);

        Assert.NotNull(repository.AddedTicket);
        Assert.Equal(request.Subject, repository.AddedTicket.Subject);
        Assert.Equal(request.Description, repository.AddedTicket.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UsesCurrentUtcTimeFromTimeProvider()
    {
        var repository = new StubSupportTicketRepository();
        var timeProvider = new FixedTimeProvider(OpenedAtUtc);
        var useCase = new CreateSupportTicket(
            new StubCurrentCustomer(AliceCustomerId),
            repository,
            timeProvider);

        await useCase.ExecuteAsync(CreateRequest());

        Assert.NotNull(repository.AddedTicket);
        Assert.Equal(OpenedAtUtc, repository.AddedTicket.OpenedAtUtc);
        Assert.Equal(1, timeProvider.GetUtcNowCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsCreatedTicket()
    {
        var repository = new StubSupportTicketRepository();
        var useCase = CreateUseCase(repository);

        await useCase.ExecuteAsync(CreateRequest());

        Assert.Equal(1, repository.AddCallCount);
        Assert.NotNull(repository.AddedTicket);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDtoMatchingCreatedTicket()
    {
        var repository = new StubSupportTicketRepository();
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(CreateRequest());

        var ticket = Assert.IsType<SupportTicket>(repository.AddedTicket);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Subject, result.Subject);
        Assert.Equal(ticket.Description, result.Description);
        Assert.Equal(ticket.Status.ToString(), result.Status);
        Assert.Equal(ticket.OpenedAtUtc, result.OpenedAtUtc);
    }

    private static CreateSupportTicket CreateUseCase(
        StubSupportTicketRepository repository) =>
        new(
            new StubCurrentCustomer(AliceCustomerId),
            repository,
            new FixedTimeProvider(OpenedAtUtc));

    private static CreateSupportTicketRequest CreateRequest() =>
        new(
            "Unable to export project",
            "The export starts but never completes.");

    private sealed class StubCurrentCustomer(Guid customerId) : ICurrentCustomer
    {
        public Guid CustomerId { get; } = customerId;
    }

    private sealed class StubSupportTicketRepository : ISupportTicketRepository
    {
        public SupportTicket? AddedTicket { get; private set; }

        public int AddCallCount { get; private set; }

        public Task AddAsync(
            SupportTicket supportTicket,
            CancellationToken cancellationToken = default)
        {
            AddedTicket = supportTicket;
            AddCallCount++;

            return Task.CompletedTask;
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
