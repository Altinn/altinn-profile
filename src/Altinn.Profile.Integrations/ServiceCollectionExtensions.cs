using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Extension class for <see cref="IServiceCollection"/> to add services and configurations.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string _profileDbAdminUserNameKey = "PostgreSqlSettings--ProfileDbAdminUserName";
    private const string _profileDbAdminPasswordKey = "PostgreSqlSettings--ProfileDbAdminPassword";
    private const string _profileDbConnectionStringKey = "PostgreSqlSettings--ProfileDbConnectionString";

    /// <summary>
    /// Adds SBL Bridge clients and configurations to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the required SblBridgeSettings are missing from the configuration.</exception>
    public static void AddSblBridgeClients(this IServiceCollection services, IConfiguration config)
    {
        var sblBridgeSettings = config.GetSection(nameof(SblBridgeSettings)).Get<SblBridgeSettings>();
        if (sblBridgeSettings == null)
        {
            throw new ArgumentNullException(nameof(config), "Required SblBridgeSettings is missing from application configuration");
        }

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

        services.AddScoped<IRegisterService, RegisterService>();

        var connectionString = config.GetDatabaseConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not properly configured.");
        }

        services.AddDbContext<ProfileDbContext>(options => options.UseNpgsql(connectionString));
    }

    /// <summary>
    /// Retrieves the database connection string from the configuration.
    /// </summary>
    /// <param name="config">The configuration instance containing the connection settings.</param>
    /// <returns>The formatted database connection string if all required settings are present; otherwise, an empty string.</returns>
    /// <remarks>
    /// This method expects the configuration to contain the following keys:
    /// <list type="bullet">
    /// <item><description><c>PostgreSqlSettings--ProfileDbAdminUserName</c></description></item>
    /// <item><description><c>PostgreSqlSettings--ProfileDbAdminPassword</c></description></item>
    /// <item><description><c>PostgreSqlSettings--ProfileDbConnectionString</c></description></item>
    /// </list>
    /// The connection string is formatted using the administrator user name and password.
    /// </remarks>
    public static string GetDatabaseConnectionString(this IConfiguration config)
    {
        var adminUserName = config[_profileDbAdminUserNameKey];
        var adminPassword = config[_profileDbAdminPasswordKey];
        var connectionString = config[_profileDbConnectionStringKey];

        if (string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        return string.Format(connectionString, adminUserName, adminPassword);
    }
}
