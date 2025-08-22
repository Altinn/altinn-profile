using Altinn.Authorization.ServiceDefaults.Leases;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class LeaseServiceCollectionExtensions
{
    /// <summary>
    /// Adds a <see cref="LeaseManager"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddLeaseManager(this IServiceCollection services)
    {
        services.TryAddTransient<LeaseManager>();

        return services;
    }
}
