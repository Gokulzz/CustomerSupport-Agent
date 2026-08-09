namespace CustomerSupport.Application.Common;

public sealed record Error(string Code, string Description)
{
    public static Error None { get; } = new(string.Empty, string.Empty);
}
