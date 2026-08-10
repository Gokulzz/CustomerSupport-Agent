using CustomerSupport.Domain.Customers;
using CustomerSupport.Domain.Invoices;
using CustomerSupport.Domain.Plans;
using CustomerSupport.Domain.Subscriptions;
using CustomerSupport.Domain.SupportTickets;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Infrastructure.Persistence
{
    public sealed class SupportDbContext : DbContext
    {
        public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Plan> Plans => Set<Plan>();

        public DbSet<Subscription> Subscriptions => Set<Subscription>();

        public DbSet<Invoice> Invoices => Set<Invoice>();

        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly);
        }
    }
}
