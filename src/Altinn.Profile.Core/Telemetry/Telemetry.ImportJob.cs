using System.Diagnostics;

using static Altinn.Profile.Core.Telemetry.Telemetry.Favorites;
using static Altinn.Profile.Core.Telemetry.Telemetry.NotificationSettings;
using static Altinn.Profile.Core.Telemetry.Telemetry.ProfileSettings;
using static Altinn.Profile.Core.Telemetry.Telemetry.SIUserContactSettings;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for the contact register update job.
/// </summary>
partial class Telemetry
{
    private const string _metricNameAdded = "added";
    private const string _metricNameUpdated = "updated";
    private const string _metricNameDeleted = "deleted";

    private void InitFavoriteImportJob(InitContext context)
    {
        InitMetricCounter(context, MetricNameFavoriteAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameFavoriteDeleted, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of favorites created.
    /// </summary>
    public void FavoriteAdded() => _counters[MetricNameFavoriteAdded].Add(1);

    /// <summary>
    /// Increments the counter for the number of favorites deleted.
    /// </summary>
    public void FavoriteDeleted() => _counters[MetricNameFavoriteDeleted].Add(1);

    private void InitNotificationSettingImportJob(InitContext context)
    {
        InitMetricCounter(context, MetricNameNotificationSettingsAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameNotificationSettingsUpdated, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameNotificationSettingsDeleted, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of notificationAddresses deleted.
    /// </summary>
    public void NotificationAddressDeleted() => _counters[MetricNameNotificationSettingsDeleted].Add(1);

    /// <summary>
    /// Increments the counter for the number of notificationAddresses updated.
    /// </summary>
    public void NotificationAddressUpdated() => _counters[MetricNameNotificationSettingsUpdated].Add(1);

    /// <summary>
    /// Increments the counter for the number of notificationAddresses added.
    /// </summary>
    public void NotificationAddressAdded() => _counters[MetricNameNotificationSettingsAdded].Add(1);

    private void InitProfileSettingImportJob(InitContext context)
    {
        InitMetricCounter(context, MetricNameProfileSettingsAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameProfileSettingsUpdated, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of ProfileSettings updated.
    /// </summary>
    public void ProfileSettingsUpdated() => _counters[MetricNameProfileSettingsUpdated].Add(1);

    /// <summary>
    /// Increments the counter for the number of ProfileSettings added.
    /// </summary>
    public void ProfileSettingsAdded() => _counters[MetricNameProfileSettingsAdded].Add(1);

    private void InitSIUserContactSettingsImportJob(InitContext context)
    {
        InitMetricCounter(context, MetricNameSIUserContactSettingsAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameSIUserContactSettingsUpdated, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of SI user contact settings added.
    /// </summary>
    public void SiUserContactSettingsAdded() => _counters[SIUserContactSettings.MetricNameSIUserContactSettingsAdded].Add(1);

    /// <summary>
    /// Increments the counter for the number of SI user contact settings updated.
    /// </summary>
    public void SiUserContactSettingsUpdated() => _counters[SIUserContactSettings.MetricNameSIUserContactSettingsUpdated].Add(1);

    /// <summary>
    /// Starts a telemetry activity for the contact registry update job.
    /// </summary>
    /// <returns>The started activity.</returns>
    public Activity? StartA2ImportJob(string jobName)
    {
        var activity = ActivitySource.StartActivity($"A2ImportJob.{jobName}");
        return activity;
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the favorites sync job.
    /// </summary>
    internal static class Favorites
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the favorites sync.
        /// </summary>
        internal const string ActivityPrefix = "Favorites";

        /// <summary>
        /// The name of the metric for the number of favorites added through the sync job.
        /// </summary>
        internal static readonly string MetricNameFavoriteAdded = MetricName(_metricNameAdded);

        /// <summary>
        /// The name of the metric for the number of favorites deleted through the sync job.
        /// </summary>
        internal static readonly string MetricNameFavoriteDeleted = MetricName(_metricNameDeleted);

        private static string MetricName(string name) => Metrics.CreateName($"favorites.{name}");
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the notification settings sync job.
    /// </summary>
    internal static class NotificationSettings
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the notification settings sync.
        /// </summary>
        internal const string ActivityPrefix = "NotificationSettings";

        /// <summary>
        /// The name of the metric for the number of favorites added through the sync job.
        /// </summary>
        internal static readonly string MetricNameNotificationSettingsAdded = MetricName(_metricNameAdded);

        /// <summary>
        /// The name of the metric for the number of favorites added through the sync job.
        /// </summary>
        internal static readonly string MetricNameNotificationSettingsUpdated = MetricName(_metricNameUpdated);

        /// <summary>
        /// The name of the metric for the number of favorites deleted through the sync job.
        /// </summary>
        internal static readonly string MetricNameNotificationSettingsDeleted = MetricName(_metricNameDeleted);

        private static string MetricName(string name) => Metrics.CreateName($"notificationsettings.{name}");
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the profile settings sync job.
    /// </summary>
    internal static class ProfileSettings
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the profile settings sync.
        /// </summary>
        internal const string ActivityPrefix = "ProfileSettings";

        /// <summary>
        /// The name of the metric for the number of profileSettings added through the sync job.
        /// </summary>
        internal static readonly string MetricNameProfileSettingsAdded = MetricName(_metricNameAdded);

        /// <summary>
        /// The name of the metric for the number of profileSettings updated through the sync job.
        /// </summary>
        internal static readonly string MetricNameProfileSettingsUpdated = MetricName(_metricNameUpdated);

        private static string MetricName(string name) => Metrics.CreateName($"profilesettings.{name}");
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the si users contact settings sync job.
    /// </summary>
    internal static class SIUserContactSettings
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the si users contact settings sync.
        /// </summary>
        internal const string ActivityPrefix = "SiUserContactSettings";

        /// <summary>
        /// The name of the metric for the number of si users contact settings added through the sync job.
        /// </summary>
        internal static readonly string MetricNameSIUserContactSettingsAdded = MetricName(_metricNameAdded);

        /// <summary>
        /// The name of the metric for the number of si users contact settings updated through the sync job.
        /// </summary>
        internal static readonly string MetricNameSIUserContactSettingsUpdated = MetricName(_metricNameUpdated);

        private static string MetricName(string name) => Metrics.CreateName($"siusercontactsettings.{name}");
    }
}
