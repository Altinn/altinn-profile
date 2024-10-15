using Microsoft.Extensions.Configuration;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for <see cref="IConfiguration"/> to add more members.
/// </summary>
public static class ConfigurationExtensions
{
    private const string ProfileDbAdminUserNameKey = "PostgreSqlSettings:ProfileDbAdminUserName";
    private const string ProfileDbAdminPasswordKey = "PostgreSqlSettings:ProfileDbAdminPassword";
    private const string ProfileDbConnectionStringKey = "PostgreSqlSettings:ProfileDbConnectionString";

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
        var adminUserName = config[ProfileDbAdminUserNameKey];
        var adminPassword = config[ProfileDbAdminPasswordKey];
        var connectionString = config[ProfileDbConnectionStringKey];

        if (string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        return string.Format(connectionString, adminUserName, adminPassword);
    }
}
