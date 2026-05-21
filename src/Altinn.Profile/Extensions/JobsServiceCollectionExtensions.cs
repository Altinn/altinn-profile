using System;
using System.Diagnostics.CodeAnalysis;

using Altinn.Authorization.ServiceDefaults.Leases;
using Altinn.Profile.Changelog;
using Altinn.Profile.Integrations.Leases;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Altinn.Profile.Jobs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Profile.Extensions;

/// <summary>
/// Extension methods for configuring job services in an <see cref="IServiceCollection"/>.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class JobsServiceCollectionExtensions
{
    /// <summary>
    /// Adds services to support leasing mechanism for jobs to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    internal static void SetupLeasing(this IServiceCollection services)
    {
        services.AddSingleton<ILeaseProvider, PostgresqlLeaseProvider>();
        services.AddSingleton<ILeaseRepository, LeaseRepository>();
        services.AddLeaseManager();
    }

    /// <summary>
    /// Adds import job services to the service collection based on configuration settings.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The configuration containing import job settings.</param>
    internal static void AddImportJobs(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient<IChangeLogClient, ChangeLogClient>();
        services.AddScoped<IChangelogSyncMetadataRepository, ChangelogSyncMetadataRepository>();

        if (config.GetValue<bool>("ImportJobSettings:FavoritesImportEnabled"))
        {
            services.AddScoped<IFavoriteSyncRepository, FavoriteSyncRepository>();

            services.AddRecurringJob<FavoriteImportJob>(settings =>
            {
                settings.LeaseName = LeaseNames.A2FavoriteImport;
                settings.Interval = TimeSpan.FromMinutes(1);
            });
        }

        if (config.GetValue<bool>("ImportJobSettings:NotificationSettingsImportEnabled"))
        {
            services.AddRecurringJob<NotificationSettingImportJob>(settings =>
            {
                settings.LeaseName = LeaseNames.A2NotificationSettingImport;
                settings.Interval = TimeSpan.FromMinutes(1);
            });
        }

        if (config.GetValue<bool>("ImportJobSettings:ProfileSettingsImportEnabled"))
        {
            services.AddRecurringJob<ProfileSettingImportJob>(settings =>
            {
                settings.LeaseName = LeaseNames.A2ProfileSettingImport;
                settings.Interval = TimeSpan.FromMinutes(1);
            });
        }

        if (config.GetValue<bool>("ImportJobSettings:SIUserAddressImportEnabled"))
        {
            services.AddRecurringJob<SIUserAddressImportJob>(settings =>
            {
                settings.LeaseName = LeaseNames.SIUserAddressImport;
                settings.Interval = TimeSpan.FromMinutes(1);
            });
        }
    }

    /// <summary>
    /// Adds import job services to the service collection based on configuration settings.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The configuration containing import job settings.</param>
    internal static void AddSyncJobs(this IServiceCollection services, IConfiguration config)
    {
        JobSettings settings = config.GetSection("JobSettings").Get<JobSettings>();

        if (settings.KrrSyncEnabled)
        {
            services.AddRecurringJob<KrrSyncJob>(jobSettings =>
            {
                jobSettings.LeaseName = JobLeaseNames.KrrSyncJob;
                jobSettings.Interval = TimeSpan.FromMinutes(settings.KrrSyncWaitTimeInMinutes);
            });
        }

        if (settings.OrgSyncEnabled)
        {
            services.AddRecurringJob<OrgSyncJob>(jobSettings =>
            {
                jobSettings.LeaseName = JobLeaseNames.OrgSyncJob;
                jobSettings.Interval = TimeSpan.FromMinutes(settings.OrgSyncWaitTimeInMinutes);
            });
        }
    }
}
