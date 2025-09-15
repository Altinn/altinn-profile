using System.Diagnostics.CodeAnalysis;

using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.Common.AccessTokenClient.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Authorization;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Extensions;
using Altinn.Profile.Integrations.Notifications;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Register;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Unit.Profile;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
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
        services.AddHttpClient<IUserProfileClient, UserProfileClient>();
        services.AddHttpClient<IUnitProfileRepository, UnitProfileClient>();
        services.AddHttpClient<IUserFavoriteClient, UserFavoriteClient>();
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

        services.Configure<AccessTokenSettings>(config.GetSection("AccessTokenSettings"));
        services.AddTransient<IAccessTokenGenerator, AccessTokenGenerator>();
        services.AddTransient<ISigningCredentialsResolver, SigningCredentialsResolver>();

        services.Configure<RegisterSettings>(config.GetSection(nameof(RegisterSettings)));
        services.AddHttpClient<IRegisterClient, RegisterClient>();
        services.Configure<NotificationsSettings>(config.GetSection(nameof(NotificationsSettings)));
        services.AddHttpClient<INotificationsClient, NotificationsClient>();
        services.AddHttpClient<IAuthorizationClient, AuthorizationClient>();

        services.AddScoped<IPersonService, PersonRepository>();
        services.AddScoped<IPersonUpdater, PersonRepository>();
        services.AddScoped<IMetadataRepository, MetadataRepository>();
        services.AddScoped<IContactRegisterUpdateJob, ContactRegisterUpdateJob>();

        services.AddSingleton<INationalIdentityNumberChecker, NationalIdentityNumberChecker>();

        services.AddScoped<IOrganizationNotificationAddressUpdater, OrganizationNotificationAddressRepository>();
        services.AddScoped<IOrganizationNotificationAddressRepository, OrganizationNotificationAddressRepository>();
        services.AddScoped<IRegistrySyncMetadataRepository, RegistrySyncMetadataRepository>();
        services.AddScoped<IOrganizationNotificationAddressSyncJob, OrganizationNotificationAddressUpdateJob>();

        services.AddScoped<IPartyGroupRepository, PartyGroupRepository>();
        services.AddScoped<IProfessionalNotificationsRepository, ProfessionalNotificationsRepository>();
        services.AddScoped<IProfessionalNotificationSyncRepository, ProfessionalNotificationsRepository>();

        services.AddDbContextFactory<ProfileDbContext>(options => options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());
    }

    /// <summary>
    /// Adds the Maskinporten client to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any of the required configuration values are missing or empty.</exception>
    public static void AddMaskinportenClient(this IServiceCollection services, IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Configuration cannot be null.");
        }

        var contactRegisterSettings = new ContactRegisterSettings();
        config.GetSection("ContactAndReservationSettings").Bind(contactRegisterSettings);
        if (contactRegisterSettings.MaskinportenSettings == null)
        {
            throw new InvalidOperationException("Contact and reservation settings are not properly configured.");
        }

        services.AddSingleton(contactRegisterSettings);
        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, IContactRegisterHttpClient, ContactRegisterHttpClient>(contactRegisterSettings.MaskinportenSettings);

        var organizationNotificationAddressSettings = new OrganizationNotificationAddressSettings();
        config.GetSection("OrganizationNotificationAddressSettings").Bind(organizationNotificationAddressSettings);
        if (string.IsNullOrWhiteSpace(organizationNotificationAddressSettings.ChangesLogEndpoint))
        {
            throw new InvalidOperationException("Organization notification address settings are not properly configured.");
        }

        services.AddSingleton(organizationNotificationAddressSettings);
        services.AddScoped<IOrganizationNotificationAddressSyncClient, OrganizationNotificationAddressHttpClient>();
        services.AddScoped<IOrganizationNotificationAddressUpdateClient, OrganizationNotificationAddressHttpClient>();
    }
}
