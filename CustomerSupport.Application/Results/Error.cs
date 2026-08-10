namespace CustomerSupport.Application.Results;

public sealed record Error(string Code, string Description)
{
    public static Error None { get; } = new(string.Empty, string.Empty);
}
