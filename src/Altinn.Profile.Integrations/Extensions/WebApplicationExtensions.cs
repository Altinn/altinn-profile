using System.Diagnostics.CodeAnalysis;

using Altinn.Profile.Integrations.Persistence;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations.Extensions;

/// <summary>
/// Extension class for web application
/// </summary>
[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    /// <summary>
    /// Run database migrations using Entity Framework Core with admin connection string.
    /// This method creates a temporary DbContext using the admin connection string
    /// to ensure migrations can be applied with proper privileges.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="config">The configuration collection</param>
    /// <returns>A task that completes when migrations are applied</returns>
    public static async Task RunDatabaseMigrationsAsync(this WebApplication app, IConfiguration config)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

        var settings = config.GetSection("PostgreSqlSettings").Get<PostgreSqlSettings>()
            ?? throw new ArgumentNullException(nameof(config), "Required PostgreSqlSettings is missing from application configuration");

        if (!settings.EnableDBConnection)
        {
            logger.LogWarning("Database connection is disabled, skipping migrations");
            return;
        }

        try
        {
            logger.LogInformation("Database migration started");

            var adminConnectionString = config.GetAdminDatabaseConnectionString();
            var options = new DbContextOptionsBuilder<ProfileDbContext>()
                .UseNpgsql(adminConnectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            using var context = new ProfileDbContext(options);
            var pendingCount = (await context.Database.GetPendingMigrationsAsync()).Count();

            if (pendingCount > 0)
            {
                logger.LogInformation("Applying {PendingMigrationCount} pending migrations", pendingCount);
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed");
            throw;
        }
    }
}
