using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ContactPoints;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Core.Extensions;

/// <summary>
/// Extension class for <see cref="IServiceCollection"/>
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core services and configurations to DI container.
    /// </summary>
    /// <param name="services">service collection.</param>
    /// <param name="config">the configuration collection</param>
    public static void AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        services
            .Configure<CoreSettings>(config.GetSection(nameof(CoreSettings)))
            .AddMemoryCache()
            .AddScoped<IUserProfileService, UserProfileService>()
            .AddScoped<IUserProfileSettingsService, UserProfileService>()
            .AddScoped<IUserContactPointsService, UserContactPointService>()
            .Decorate<IUserProfileService, UserProfileCachingDecorator>()
            .AddScoped<IUnitContactPointsService, UnitContactPointService>()
            .AddScoped<IOrganizationNotificationAddressesService, OrganizationNotificationAddressesService>()
            .AddScoped<IPartyGroupService, PartyGroupService>()
            .AddScoped<IProfessionalNotificationsService, ProfessionalNotificationsService>();
    }
}
