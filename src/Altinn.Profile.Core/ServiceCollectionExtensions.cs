using Altinn.Profile.Core;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ContactPoints;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Notifications.Core.Extensions;

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
            .AddSingleton<IUserProfileService, UserProfileService>()
            .AddSingleton<IUserContactPoints, UserContactPointService>()
            .Decorate<IUserProfileService, UserProfileCachingDecorator>()
            .AddSingleton<IUnitContactPoints, UnitContactPointService>();
    }
}
