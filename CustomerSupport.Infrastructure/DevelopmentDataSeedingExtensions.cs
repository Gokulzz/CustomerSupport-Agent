using CustomerSupport.Infrastructure.Persistence.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerSupport.Infrastructure;

public static class DevelopmentDataSeedingExtensions
{
    public static async Task SeedDevelopmentDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();

        await seeder.SeedAsync(cancellationToken);
    }
}
