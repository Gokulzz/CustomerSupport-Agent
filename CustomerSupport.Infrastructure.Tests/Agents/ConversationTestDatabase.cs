using CustomerSupport.Domain.Customers;
using CustomerSupport.Domain.Conversation;
using CustomerSupport.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CustomerSupport.Infrastructure.Tests.Agents;

// A private relational database per test; never connects to the development SQL Server.
internal sealed class ConversationTestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly DbContextOptions<SupportDbContext> options;

    public ConversationTestDatabase(Guid customerId)
    {
        connection.Open();
        options = new DbContextOptionsBuilder<SupportDbContext>().UseSqlite(connection)
            .ReplaceService<IModelCustomizer, SqliteUtcModelCustomizer>().Options;
        Context = CreateContext();
        Context.Database.EnsureCreated();
        Context.Customers.Add(Customer.Create(customerId, "Test customer"));
        Context.SaveChanges();
    }

    public SupportDbContext Context { get; }
    public SupportDbContext CreateContext() => new(options);

    public void Dispose()
    {
        Context.Dispose();
        connection.Dispose();
    }
}

// SQLite cannot order DateTimeOffset columns natively. Store UTC ticks for these tests only.
internal sealed class SqliteUtcModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        modelBuilder.Entity<Conversation>().Property(x => x.CreatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<Conversation>().Property(x => x.UpdatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<ConversationMessage>().Property(x => x.CreatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
    }
}
