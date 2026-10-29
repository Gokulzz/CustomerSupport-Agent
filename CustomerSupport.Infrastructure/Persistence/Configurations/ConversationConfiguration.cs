

using CustomerSupport.Domain.Conversation;
using CustomerSupport.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSupport.Infrastructure.Persistence.Configurations
{
    internal class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("Conversations");
            builder.HasKey(conversation => conversation.Id);
            builder.Property(conversation => conversation.Id).ValueGeneratedNever();
            builder.Property(conversation => conversation.CustomerId).IsRequired();
            builder.Property(conversation => conversation.CreatedAtUtc).IsRequired();
            builder.Property(conversation => conversation.UpdatedAtUtc);
            builder.HasOne<Customer>()
                   .WithMany()
                   .HasForeignKey(conversation => conversation.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);    
     
        }   
    }
}
