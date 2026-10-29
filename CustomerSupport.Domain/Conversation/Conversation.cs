
using CustomerSupport.Domain.Conversation.Enum;

namespace CustomerSupport.Domain.Conversation
{
    public sealed class Conversation
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        private readonly List<ConversationMessage> _messages = new();
        public IReadOnlyCollection<ConversationMessage> Messages => _messages; 

        private Conversation() { }

        public static Conversation Create(Guid customerId, DateTimeOffset dateTime)
        {
            return new Conversation
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CreatedAtUtc = dateTime,
                UpdatedAtUtc = dateTime
            };
        }  
        public void AddMessage(ConversationMessageRole messagRole, string content, DateTimeOffset now)  
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(content);
           var message = ConversationMessage.Create(Id, messagRole, content, now);
            _messages.Add(message);
            UpdatedAtUtc = now;


        }   
    }
}
