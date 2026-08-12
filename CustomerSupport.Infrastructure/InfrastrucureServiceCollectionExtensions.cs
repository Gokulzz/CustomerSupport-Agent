using CustomerSupport.Infrastructure.Persistence;
using CustomerSupport.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerSupport.Infrastructure;

public static class InfrastrucureServiceCollectionExtensions
{
    private const string ConnectionStringName = "CustomerSupportDatabase";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<SupportDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }
}
