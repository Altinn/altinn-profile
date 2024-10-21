using System.Diagnostics.CodeAnalysis;

using Altinn.Profile.Integrations.Persistence;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using Yuniql.AspNetCore;
using Yuniql.PostgreSql;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for web application
/// </summary>
[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure and set up db
    /// </summary>
    /// <param name="app">app</param>
    /// <param name="config">the configuration collection</param>
    public static void SetUpPostgreSql(this IApplicationBuilder app, IConfiguration config)
    {
        PostgreSqlSettings? settings = config.GetSection("PostgreSQLSettings").Get<PostgreSqlSettings>()
            ?? throw new ArgumentNullException(nameof(config), "Required PostgreSQLSettings is missing from application configuration");

        if (settings.EnableDBConnection)
        {
            ConsoleTraceService traceService = new() { IsDebugEnabled = true };

            string connectionString = string.Format(settings.AdminConnectionString, settings.ProfileDbAdminPwd);

            string fullWorkspacePath = Path.Combine(Environment.CurrentDirectory, settings.MigrationScriptPath);

            app.UseYuniql(
                new PostgreSqlDataService(traceService),
                new PostgreSqlBulkImportService(traceService),
                traceService,
                new Configuration
                {
                    Workspace = fullWorkspacePath,
                    ConnectionString = connectionString,
                    IsAutoCreateDatabase = false,
                    IsDebug = settings.EnableDebug
                });
        }
    }
}
