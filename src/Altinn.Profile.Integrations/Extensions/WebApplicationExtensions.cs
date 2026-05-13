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
    private const string WolverineSchema = "wolverine";
    private const string RuntimeDbUser = "platform_profile";

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

    /// <summary>
    /// Grants runtime database user permissions required for Wolverine schema objects.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="config">The configuration collection.</param>
    /// <returns>A task that completes when grants are applied.</returns>
    public static async Task GrantWolverinePermissionsAsync(this WebApplication app, IConfiguration config)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
        var adminConnectionString = config.GetAdminDatabaseConnectionString();

        if (string.IsNullOrWhiteSpace(adminConnectionString))
        {
            throw new InvalidOperationException("Admin database connection string is not properly configured.");
        }

        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseNpgsql(adminConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        using var context = new ProfileDbContext(options);

        logger.LogInformation("Granting Wolverine schema permissions to runtime user");

        await context.Database.ExecuteSqlRawAsync($"GRANT USAGE ON SCHEMA {WolverineSchema} TO {RuntimeDbUser};");
        await context.Database.ExecuteSqlRawAsync($"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {WolverineSchema} TO {RuntimeDbUser};");
        await context.Database.ExecuteSqlRawAsync($"GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA {WolverineSchema} TO {RuntimeDbUser};");
        await context.Database.ExecuteSqlRawAsync($"ALTER DEFAULT PRIVILEGES IN SCHEMA {WolverineSchema} GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {RuntimeDbUser};");
        await context.Database.ExecuteSqlRawAsync($"ALTER DEFAULT PRIVILEGES IN SCHEMA {WolverineSchema} GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO {RuntimeDbUser};");

        logger.LogInformation("Wolverine schema permissions granted successfully");
    }
}
