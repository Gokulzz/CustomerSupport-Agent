using CustomerSupport.Domain.Conversation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CustomerSupport.Infrastructure.Persistence.Configurations
{
    internal class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
    {
        public void Configure(EntityTypeBuilder<ConversationMessage> builder)
        {
            builder.ToTable("ConversationMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Id).ValueGeneratedNever();
            builder.Property(message => message.ConversationId).IsRequired();
            builder.Property(message => message.Role).IsRequired();
            builder.Property(message => message.Content).IsRequired();
            builder.Property(message => message.CreatedAtUtc).IsRequired();
            builder.HasOne<Conversation>()
                   .WithMany(conversation => conversation.Messages)
                   .HasForeignKey(message => message.ConversationId)
                   .OnDelete(DeleteBehavior.Restrict);  
     

        }   
    }
}
