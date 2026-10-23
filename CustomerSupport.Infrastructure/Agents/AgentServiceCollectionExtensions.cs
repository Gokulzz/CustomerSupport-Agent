#pragma warning disable OPENAI001 // Responses API is marked experimental by the official SDK.
using CustomerSupport.Application.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Responses;

namespace CustomerSupport.Infrastructure.Agents;

internal static class AgentServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerSupportAgent(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OpenAIOptions>()
            .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "OpenAI:Model is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "OpenAI:ApiKey must be configured through user secrets or environment variables.");

        // Lazy creation lets the rest of the backend run before an OpenAI key is supplied.
        services.AddSingleton(provider =>
            new ResponsesClient(provider.GetRequiredService<IOptions<OpenAIOptions>>().Value.ApiKey));
        services.AddScoped<AgentToolDispatcher>();
        services.AddScoped<ICustomerSupportAgent, OpenAICustomerSupportAgent>();
        return services;
    }
}
