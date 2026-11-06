

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
            if (conversationId == Guid.Empty)
                throw new ArgumentException("Conversation ID cannot be empty.", nameof(conversationId));
            if (!System.Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(role));
            ArgumentException.ThrowIfNullOrWhiteSpace(content);

            return new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = role,
                Content = content,
                CreatedAtUtc = dateTime.ToUniversalTime()
            };
        }


    }


}
