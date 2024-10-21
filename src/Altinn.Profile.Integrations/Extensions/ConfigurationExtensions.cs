using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for <see cref="IConfiguration"/> to add more members.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    private const string ConnectionStringKey = "PostgreSqlSettings:ConnectionString";
    private const string ProfileDbPasswordKey = "PostgreSqlSettings:ProfileDbPwd";

    /// <summary>
    /// Retrieves the database connection string from the configuration.
    /// </summary>
    /// <param name="config">The configuration instance containing the connection settings.</param>
    /// <returns>The formatted database connection string if all required settings are present; otherwise, an empty string.</returns>
    /// <remarks>
    /// This method expects IConfiguration to contain the following keys:
    /// <list type="bullet">
    /// <item><description><c>PostgreSqlSettings:ConnectionString</c></description></item>
    /// <item><description><c>PostgreSqlSettings:ProfileDbPwd</c></description></item>
    /// </list>
    /// The connection string is expected to contain a placeholder for the password. The password is added to the connection string
    /// through string formatting. The connection string value should be provieded as a value in the helm chart for profile. The password
    /// is retrieved from the platform KeyVault. The names can therefore not be changed, but must follow the above naming conventions.
    /// </remarks>
    public static string GetDatabaseConnectionString(this IConfiguration config)
    {
        var connectionString = config[ConnectionStringKey];
        var userPassword = config[ProfileDbPasswordKey];

        if (string.IsNullOrWhiteSpace(userPassword) ||
            string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        return string.Format(connectionString, userPassword);
    }
}
