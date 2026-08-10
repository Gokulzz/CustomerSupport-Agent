using CustomerSupport.Domain.Customers;
using CustomerSupport.Domain.Plans;
using CustomerSupport.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSupport.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(subscription => subscription.Id);
        builder.Property(subscription => subscription.Id).ValueGeneratedNever();
        builder.Property(subscription => subscription.CustomerId).IsRequired();
        builder.Property(subscription => subscription.PlanId).IsRequired();

        builder.Property(subscription => subscription.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(subscription => subscription.RenewsAtUtc);
        builder.Ignore(subscription => subscription.IsActive);


        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(subscription => subscription.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(subscription => subscription.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
