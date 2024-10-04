using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for <see cref="IServiceCollection"/> to add services and configurations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SBL Bridge clients and configurations to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the required SblBridgeSettings are missing from the configuration.</exception>
    public static void AddSblBridgeClients(this IServiceCollection services, IConfiguration config)
    {
        var sblBridgeSettings = config.GetSection(nameof(SblBridgeSettings)).Get<SblBridgeSettings>() ?? throw new ArgumentNullException(nameof(config), "Required SblBridgeSettings is missing from application configuration");
        services.Configure<SblBridgeSettings>(config.GetSection(nameof(SblBridgeSettings)));

        services.AddHttpClient<IUserProfileClient, UserProfileClient>();
        services.AddHttpClient<IUnitProfileClient, UnitProfileClient>();
    }

    /// <summary>
    /// Adds the register service and database context to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any of the required configuration values are missing or empty.</exception>
    public static void AddRegisterService(this IServiceCollection services, IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Configuration cannot be null.");
        }

        var connectionString = config.GetDatabaseConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not properly configured.");
        }

        services.AddDbContext<ProfileDbContext>(options => options.UseNpgsql(connectionString));

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRegisterRepository, RegisterRepository>();

        services.AddSingleton<INationalIdentityNumberChecker, NationalIdentityNumberChecker>();
    }
}
