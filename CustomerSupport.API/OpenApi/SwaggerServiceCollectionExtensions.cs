using Microsoft.OpenApi;

namespace CustomerSupport.Api.OpenApi;

internal static class SwaggerServiceCollectionExtensions
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerWithBearerAuthentication(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter the JWT access token. Do not include the 'Bearer' prefix."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerScheme, document, externalResource: null)] = []
            });
        });

        return services;
    }
}
