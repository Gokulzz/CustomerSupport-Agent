

using CustomerSupport.Domain.Conversation.Enum;

namespace CustomerSupport.Domain.Conversation
{
    public sealed class ConversationMessage
    {
        public Guid Id { get; private set; }
        public Guid ConversationId { get; private set; }
        public ConversationMessageRole Role { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        private ConversationMessage() { }
        public static ConversationMessage Create(Guid conversationId, ConversationMessageRole role, string content, DateTimeOffset dateTime)
        {
            return new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = role,
                Content = content,
                CreatedAtUtc = dateTime
            };
        }   

      
    }

    
    }
