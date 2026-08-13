using CustomerSupport.Application.Authentication;
using CustomerSupport.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CustomerSupport.Api.Authentication;

internal static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", LoginAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        IAccessTokenService accessTokenService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null ||
            user.Email is null ||
            !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Unauthorized();
        }

        var accessToken = accessTokenService.Create(
            user.Id,
            user.CustomerId,
            user.Email);

        return Results.Ok(new LoginResponse(
            accessToken.Value,
            "Bearer",
            accessToken.ExpiresAtUtc));
    }

}

internal sealed record LoginRequest(string Email, string Password);

internal sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc);
