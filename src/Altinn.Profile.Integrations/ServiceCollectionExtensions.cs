﻿using System.Diagnostics.CodeAnalysis;

using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Integrations.Mappings;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;
using Altinn.Profile.Integrations.SblBridge.User.Profile;
using Altinn.Profile.Integrations.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Extension class for <see cref="IServiceCollection"/>
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Altinn clients and configurations to DI container.
    /// </summary>
    /// <param name="services">service collection.</param>
    /// <param name="config">the configuration collection</param>
    public static void AddSblBridgeClients(this IServiceCollection services, IConfiguration config)
    {
        _ = config.GetSection(nameof(SblBridgeSettings))
            .Get<SblBridgeSettings>()
            ?? throw new ArgumentNullException(nameof(config), "Required SblBridgeSettings is missing from application configuration");

        services.Configure<SblBridgeSettings>(config.GetSection(nameof(SblBridgeSettings)));
        services.AddHttpClient<IUserProfileRepository, UserProfileClient>();
        services.AddHttpClient<IUnitProfileRepository, UnitProfileClient>();
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

        services.AddAutoMapper(typeof(PersonContactDetailsProfile));

        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IPersonRepository, PersonRepository>();

        services.AddSingleton<INationalIdentityNumberChecker, NationalIdentityNumberChecker>();
    }
}
