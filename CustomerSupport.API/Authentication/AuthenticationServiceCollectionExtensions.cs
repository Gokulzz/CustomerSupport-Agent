using CustomerSupport.Application.Authentication;

namespace CustomerSupport.Api.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentCustomer, HttpContextCurrentCustomer>();

        return services;
    }
}
