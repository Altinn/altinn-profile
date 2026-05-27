using System.Diagnostics.CodeAnalysis;

using Altinn.Profile.Integrations.Persistence;

using Microsoft.Extensions.Configuration;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for <see cref="IConfiguration"/> to add more members.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    /// <summary>
    /// Retrieves the database connection string from the configuration.
    /// </summary>
    /// <param name="config">The configuration instance containing the connection settings.</param>
    /// <returns>The formatted database connection string if all required settings are present; otherwise, an empty string.</returns>
    /// <remarks>
    /// This method reads <see cref="PostgreSqlSettings.ConnectionString"/> and <see cref="PostgreSqlSettings.ProfileDbPwd"/>
    /// from the <c>PostgreSqlSettings</c> configuration section.
    /// </remarks>
    public static string GetDatabaseConnectionString(this IConfiguration config)
    {
        var settings = config.GetSection(nameof(PostgreSqlSettings)).Get<PostgreSqlSettings>();

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.ConnectionString) ||
            string.IsNullOrWhiteSpace(settings.ProfileDbPwd))
        {
            return string.Empty;
        }

        return string.Format(settings.ConnectionString, settings.ProfileDbPwd);
    }

    /// <summary>
    /// Retrieves the admin database connection string from the configuration for database migrations.
    /// </summary>
    /// <param name="config">The configuration instance containing the connection settings.</param>
    /// <returns>The formatted admin database connection string if all required settings are present; otherwise, an empty string.</returns>
    /// <remarks>
    /// This method reads <see cref="PostgreSqlSettings.AdminConnectionString"/> and <see cref="PostgreSqlSettings.ProfileDbAdminPwd"/>
    /// from the <c>PostgreSqlSettings</c> configuration section.
    /// </remarks>
    public static string GetAdminDatabaseConnectionString(this IConfiguration config)
    {
        var settings = config.GetSection(nameof(PostgreSqlSettings)).Get<PostgreSqlSettings>();

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.AdminConnectionString) ||
            string.IsNullOrWhiteSpace(settings.ProfileDbAdminPwd))
        {
            return string.Empty;
        }

        return string.Format(settings.AdminConnectionString, settings.ProfileDbAdminPwd);
    }
}
