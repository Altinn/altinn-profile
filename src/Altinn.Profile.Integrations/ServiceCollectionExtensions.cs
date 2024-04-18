using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.SblBridge;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Integrations
{
    /// <summary>
    /// Extension class for <see cref="IServiceCollection"/>
    /// </summary>
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

            services
                .Configure<SblBridgeSettings>(config.GetSection(nameof(SblBridgeSettings)))
                .AddHttpClient<IUserProfileClient, UserProfileClient>();
        }
    }
}
