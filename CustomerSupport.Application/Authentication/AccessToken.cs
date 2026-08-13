namespace CustomerSupport.Application.Authentication;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAtUtc);
