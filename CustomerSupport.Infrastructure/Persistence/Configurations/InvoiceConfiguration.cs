
using CustomerSupport.Domain.Customers;
using CustomerSupport.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSupport.Infrastructure.Persistence.Configurations
{
    internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.CustomerId).IsRequired();
            builder.Property(x => x.InvoiceNumber)
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(x => x.Amount)
                   .HasPrecision(18, 2)
                   .IsRequired();
            builder.Property(x => x.IssuedAtUtc).IsRequired();
            builder.Property(x => x.Status)
                  .HasConversion<string>()
                  .HasMaxLength(32)
                  .IsRequired();
            builder.Property(x => x.PaidAtUtc);

            builder.HasIndex(x => x.InvoiceNumber);

            builder.HasOne<Customer>()
                   .WithMany()
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
