#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Altinn.Common.AccessToken.Services;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Integrations.Handlers;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Mocks.Authentication;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Wolverine;
using Wolverine.EntityFrameworkCore;

namespace Altinn.Profile.Tests.IntegrationTests;

public class ProfileWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Force test environment to ensure Wolverine detects test mode
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.test.json");
        });

        builder.ConfigureServices(services =>
        {
            // Configure Wolverine for test environment
            services.ConfigureWolverineForTesting();

            // Add test-specific services
            services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
            
            // Add connection pool monitoring if enabled
            if (IsConnectionPoolMonitoringEnabled())
            {
                services.AddSingleton<ConnectionPoolMonitor>();
            }
        });

        // Configure logging for test environment - disable EventLog to prevent disposal issues
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders(); // Remove all providers to prevent disposal race conditions
            
            // Don't add any providers - use null logger to prevent disposal issues during test cleanup
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Force immediate shutdown of hosted services to prevent disposal race conditions
                var hostApplicationLifetime = Services.GetService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
                hostApplicationLifetime?.StopApplication();

                // Give a brief moment for cleanup
                Task.Delay(50).Wait();
            }
            catch
            {
                // Ignore cleanup errors during test disposal
            }
        }
        
        try
        {
            base.Dispose(disposing);
        }
        catch
        {
            // Ignore disposal race conditions
        }
    }

    private bool IsConnectionPoolMonitoringEnabled()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json")
            .Build();
            
        return config.GetValue<bool>("TestSettings:ConnectionPoolMonitoringEnabled", false);
    }
}

public static class WolverineTestExtensions
{
    public static IServiceCollection ConfigureWolverineForTesting(this IServiceCollection services)
    {
        // Override the Wolverine configuration in the application
        // to run the application in "solo" mode for faster
        // testing cold starts
        services.RunWolverineInSoloMode();

        // And just for completion, disable all Wolverine external 
        // messaging transports
        services.DisableAllExternalWolverineTransports();

        return services;
    }
}

public class ConnectionPoolMonitor
{
    private readonly ILogger<ConnectionPoolMonitor> _logger;

    public ConnectionPoolMonitor(ILogger<ConnectionPoolMonitor> logger)
    {
        _logger = logger;
    }

    // TODO: Implement connection pool monitoring logic
    // This could track active connections, pool utilization, etc.
}
