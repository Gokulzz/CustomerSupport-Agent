using CustomerSupport.Domain.Customers;
using CustomerSupport.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSupport.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.CustomerId).IsRequired();

        builder.HasIndex(user => user.CustomerId).IsUnique();

        builder.HasOne<Customer>()
            .WithOne()
            .HasForeignKey<ApplicationUser>(user => user.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
