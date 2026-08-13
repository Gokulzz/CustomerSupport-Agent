using System.Text;
using CustomerSupport.Application.Authentication;
using CustomerSupport.Infrastructure.Authentication;
using CustomerSupport.Infrastructure.Identity;
using CustomerSupport.Infrastructure.Persistence;
using CustomerSupport.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException(
           $"JWT options are not configured properly in the '{JwtOptions.SectionName}' section.");
        services.Configure<JwtOptions>(jwtSection);

        services.AddDbContext<SupportDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<SupportDbContext>(); 
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<DevelopmentDataSeeder>();

        return services;
    }

   
}
