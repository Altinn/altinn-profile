#nullable enable

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests;

public class MigrateOnStartupTests
{
    private const string MigrationLogCategory = "DatabaseMigration";

    [Fact]
    public async Task Start_MigrateOnStartupEnabled_RunsTheMigrationsBeforeTheAppStarts()
    {
        // Arrange
        // With the database connection disabled the migration logs a warning and returns,
        // so the test shows that it ran without a database.
        CapturingLoggerProvider logs = new();
        using WebApplicationFactory<Program> factory = CreateFactory(logs, migrateOnStartup: "true");

        // Act
        HttpStatusCode status = await GetHealthStatus(factory);

        // Assert
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(
            logs.Messages,
            m => m.Category == MigrationLogCategory && m.Message == "Database connection is disabled, skipping migrations");
    }

    [Fact]
    public async Task Start_MigrateOnStartupNotSet_DoesNotRunTheMigrations()
    {
        // Arrange
        CapturingLoggerProvider logs = new();
        using WebApplicationFactory<Program> factory = CreateFactory(logs, migrateOnStartup: null);

        // Act
        HttpStatusCode status = await GetHealthStatus(factory);

        // Assert
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.DoesNotContain(logs.Messages, m => m.Category == MigrationLogCategory);
    }

    private static WebApplicationFactory<Program> CreateFactory(CapturingLoggerProvider logs, string? migrateOnStartup)
    {
        ProfileWebApplicationFactory<Program> factory = new();
        factory.InMemoryConfigurationCollection["PostgreSqlSettings:EnableDBConnection"] = "false";
        factory.InMemoryConfigurationCollection["PostgreSqlSettings:MigrateOnStartup"] = migrateOnStartup;
        return factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<ILoggerProvider>(logs)));
    }

    private static async Task<HttpStatusCode> GetHealthStatus(WebApplicationFactory<Program> factory)
    {
        HttpClient client = factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        return response.StatusCode;
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(string Category, string Message)> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, Messages);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, ConcurrentQueue<(string Category, string Message)> messages)
            : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                messages.Enqueue((category, formatter(state, exception)));
            }
        }
    }
}
