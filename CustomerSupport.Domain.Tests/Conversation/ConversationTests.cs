using CustomerSupport.Domain.Conversation;
using CustomerSupport.Domain.Conversation.Enum;
using Xunit;

namespace CustomerSupport.Domain.Tests.Conversation;

public sealed class ConversationTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddMessageLinksToConversationAndUpdatesTimestamp()
    {
        var conversation = Domain.Conversation.Conversation.Create(Guid.NewGuid(), CreatedAt);
        conversation.AddMessage(ConversationMessageRole.User, "My plan?", CreatedAt.AddMinutes(1));

        var message = Assert.Single(conversation.Messages);
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(conversation.Id, message.ConversationId);
        Assert.Equal(ConversationMessageRole.User, message.Role);
        Assert.Equal("My plan?", message.Content);
        Assert.Equal(CreatedAt.AddMinutes(1), message.CreatedAtUtc);
        Assert.Equal(message.CreatedAtUtc, conversation.UpdatedAtUtc);
        Assert.Throws<NotSupportedException>(() => ((ICollection<ConversationMessage>)conversation.Messages).Clear());
    }

    [Fact]
    public void CreateRejectsEmptyCustomerId() =>
        Assert.Throws<ArgumentException>(() => Domain.Conversation.Conversation.Create(Guid.Empty, CreatedAt));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void BlankMessageIsRejectedWithoutChangingConversation(string? content)
    {
        var conversation = Domain.Conversation.Conversation.Create(Guid.NewGuid(), CreatedAt);
        Assert.ThrowsAny<ArgumentException>(() => conversation.AddMessage(ConversationMessageRole.User, content!, CreatedAt.AddMinutes(1)));
        Assert.Empty(conversation.Messages);
        Assert.Equal(CreatedAt, conversation.UpdatedAtUtc);
        Assert.ThrowsAny<ArgumentException>(() => ConversationMessage.Create(conversation.Id, ConversationMessageRole.User, content!, CreatedAt));
    }

    [Fact]
    public void MessageFactoryRejectsEmptyConversationIdAndUndefinedRole()
    {
        Assert.Throws<ArgumentException>(() => ConversationMessage.Create(Guid.Empty, ConversationMessageRole.User, "Hello", CreatedAt));
        Assert.Throws<ArgumentOutOfRangeException>(() => ConversationMessage.Create(Guid.NewGuid(), (ConversationMessageRole)99, "Hello", CreatedAt));
    }

    [Fact]
    public void BackdatedMessageDoesNotChangeExistingHistory()
    {
        var conversation = Domain.Conversation.Conversation.Create(Guid.NewGuid(), CreatedAt);
        conversation.AddMessage(ConversationMessageRole.User, "First", CreatedAt.AddMinutes(1));
        Assert.Throws<ArgumentException>(() => conversation.AddMessage(ConversationMessageRole.Assistant, "Backdated", CreatedAt));
        Assert.Single(conversation.Messages);
        Assert.Equal(CreatedAt.AddMinutes(1), conversation.UpdatedAtUtc);
    }

    [Fact]
    public void TimestampsAreNormalizedToUtc()
    {
        var conversation = Domain.Conversation.Conversation.Create(Guid.NewGuid(), CreatedAt.ToOffset(TimeSpan.FromHours(2)));
        conversation.AddMessage(ConversationMessageRole.User, "Hello", CreatedAt.ToOffset(TimeSpan.FromHours(2)));
        Assert.Equal(TimeSpan.Zero, conversation.CreatedAtUtc.Offset);
        Assert.Equal(TimeSpan.Zero, conversation.UpdatedAtUtc.Offset);
        Assert.Equal(TimeSpan.Zero, Assert.Single(conversation.Messages).CreatedAtUtc.Offset);
    }
}
