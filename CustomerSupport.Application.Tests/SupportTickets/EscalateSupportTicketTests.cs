using CustomerSupport.Application.Authentication;
using CustomerSupport.Application.SupportTickets;
using CustomerSupport.Domain.SupportTickets;
using Xunit;

namespace CustomerSupport.Application.Tests.SupportTickets;

public sealed class EscalateSupportTicketTests
{
    private static readonly Guid AliceCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid BobCustomerId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid AliceTicketId =
        Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid BobTicketId =
        Guid.Parse("50000000-0000-0000-0000-000000000002");
    private static readonly DateTimeOffset OpenedAtUtc =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EscalatedAtUtc =
        new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_QueriesByRequestedTicketAndAuthenticatedCustomer()
    {
        var repository = new StubSupportTicketRepository(ticket: null);
        var useCase = CreateUseCase(repository);

        await useCase.ExecuteAsync(AliceTicketId);

        Assert.Equal(AliceTicketId, repository.RequestedTicketId);
        Assert.Equal(AliceCustomerId, repository.RequestedCustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ChangesStatusToEscalated()
    {
        var ticket = CreateOpenTicket(AliceTicketId, AliceCustomerId);
        var useCase = CreateUseCase(new StubSupportTicketRepository(ticket));

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TicketStatus.Escalated, ticket.Status);
        Assert.Equal("Escalated", result.Value.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_UsesUtcTimeFromTimeProvider()
    {
        var ticket = CreateOpenTicket(AliceTicketId, AliceCustomerId);
        var repository = new StubSupportTicketRepository(ticket);
        var timeProvider = new FixedTimeProvider(EscalatedAtUtc);
        var useCase = new EscalateSupportTicket(
            new StubCurrentCustomer(AliceCustomerId),
            repository,
            timeProvider);

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsSuccess);
        Assert.Equal(EscalatedAtUtc, ticket.EscalatedAtUtc);
        Assert.Equal(EscalatedAtUtc, result.Value.EscalatedAtUtc);
        Assert.Equal(1, timeProvider.GetUtcNowCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_PersistsExactlyOnce()
    {
        var repository = new StubSupportTicketRepository(
            CreateOpenTicket(AliceTicketId, AliceCustomerId));
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ReturnsEscalatedTicketDto()
    {
        var ticket = CreateOpenTicket(AliceTicketId, AliceCustomerId);
        var useCase = CreateUseCase(new StubSupportTicketRepository(ticket));

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ticket.Id, result.Value.Id);
        Assert.Equal(ticket.Status.ToString(), result.Value.Status);
        Assert.Equal(ticket.EscalatedAtUtc, result.Value.EscalatedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTicketIsMissing_ReturnsNotFoundWithoutPersisting()
    {
        var repository = new StubSupportTicketRepository(ticket: null);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsFailure);
        Assert.Equal(SupportTicketErrors.NotFound, result.Error);
        Assert.Equal("SupportTicket.NotFound", result.Error.Code);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyEscalated_ReturnsExpectedFailureWithoutPersisting()
    {
        var ticket = CreateOpenTicket(AliceTicketId, AliceCustomerId);
        ticket.Escalate(EscalatedAtUtc.AddDays(-1));
        var repository = new StubSupportTicketRepository(ticket);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsFailure);
        Assert.Equal(SupportTicketErrors.AlreadyEscalated, result.Error);
        Assert.Equal("SupportTicket.AlreadyEscalated", result.Error.Code);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenClosed_ReturnsExpectedFailureWithoutPersisting()
    {
        var ticket = CreateOpenTicket(AliceTicketId, AliceCustomerId);
        ticket.Close(EscalatedAtUtc.AddDays(-1));
        var repository = new StubSupportTicketRepository(ticket);
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(AliceTicketId);

        Assert.True(result.IsFailure);
        Assert.Equal(SupportTicketErrors.AlreadyClosed, result.Error);
        Assert.Equal("SupportTicket.AlreadyClosed", result.Error.Code);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAliceRequestsBobsTicket_ReturnsNotFoundWithoutPersisting()
    {
        var repository = new StubSupportTicketRepository(
            CreateOpenTicket(BobTicketId, BobCustomerId));
        var useCase = CreateUseCase(repository);

        var result = await useCase.ExecuteAsync(BobTicketId);

        Assert.True(result.IsFailure);
        Assert.Equal(SupportTicketErrors.NotFound, result.Error);
        Assert.Equal(BobTicketId, repository.RequestedTicketId);
        Assert.Equal(AliceCustomerId, repository.RequestedCustomerId);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private static EscalateSupportTicket CreateUseCase(
        StubSupportTicketRepository repository) =>
        new(
            new StubCurrentCustomer(AliceCustomerId),
            repository,
            new FixedTimeProvider(EscalatedAtUtc));

    private static SupportTicket CreateOpenTicket(Guid ticketId, Guid customerId) =>
        SupportTicket.Create(
            ticketId,
            customerId,
            "Unable to export project",
            "The export starts but never completes.",
            OpenedAtUtc);

    private sealed class StubCurrentCustomer(Guid customerId) : ICurrentCustomer
    {
        public Guid CustomerId { get; } = customerId;
    }

    private sealed class StubSupportTicketRepository(SupportTicket? ticket)
        : ISupportTicketRepository
    {
        public Guid? RequestedTicketId { get; private set; }

        public Guid? RequestedCustomerId { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<SupportTicket?> GetByIdAndCustomerIdAsync(
            Guid ticketId,
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            RequestedTicketId = ticketId;
            RequestedCustomerId = customerId;

            var matches = ticket is not null
                && ticket.Id == ticketId
                && ticket.CustomerId == customerId;

            return Task.FromResult(matches ? ticket : null);
        }

        public Task AddAsync(
            SupportTicket supportTicket,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
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
