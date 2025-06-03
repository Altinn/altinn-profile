using System.Diagnostics;

using static Altinn.Profile.Core.Telemetry.Telemetry.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for the organization notification address update job.
/// </summary>
partial class Telemetry
{
    private void InitOrganizationNotificationAddressUpdateJob(InitContext context)
    {
        InitMetricCounter(context, MetricNameOrganizationAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameAddressAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameAddressUpdated, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNameAddressDeleted, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of organizations created.
    /// </summary>
    public void OrganizationAdded() => _counters[MetricNameOrganizationAdded].Add(1);

    /// <summary>
    /// Increments the counter for the number of addresses added.
    /// </summary>
    public void AddressAdded() => _counters[MetricNameAddressAdded].Add(1);

    /// <summary>
    /// Increments the counter for the number of addresses updated.
    /// </summary>
    public void AddressUpdated() => _counters[MetricNameAddressUpdated].Add(1);

    /// <summary>
    /// Increments the counter for the number of addresses deleted.
    /// </summary>
    public void AddressDeleted() => _counters[MetricNameAddressDeleted].Add(1);

    /// <summary>
    /// Starts a telemetry activity for the organization notification address update job.
    /// </summary>
    /// <returns>The started activity.</returns>
    public Activity? StartOrganizationNotificationAddressUpdateJob()
    {
        var activity = ActivitySource.StartActivity($"{ActivityPrefix}.SyncAddressesAsync");
        return activity;
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the organization notification address update job.
    /// </summary>
    internal static class OrganizationNotificationAddresses
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the organization notification address registry.
        /// </summary>
        internal const string ActivityPrefix = "OrganizationNotificationAddress";

        /// <summary>
        /// The name of the metric for the number of organizations added through the sync job.
        /// </summary>
        internal static readonly string MetricNameOrganizationAdded = MetricName("organization.added");

        /// <summary>
        /// The name of the metric for the number of notification addresses added through the sync job.
        /// </summary>
        internal static readonly string MetricNameAddressAdded = MetricName("address.added");

        /// <summary>
        /// The name of the metric for the number of notification addresses updated through the sync job.
        /// </summary>
        internal static readonly string MetricNameAddressUpdated = MetricName("address.updated");

        /// <summary>
        /// The name of the metric for the number of notification addresses deleted through the sync job.
        /// </summary>
        internal static readonly string MetricNameAddressDeleted = MetricName("address.deleted");

        private static string MetricName(string name) => Metrics.CreateName($"organizationnotificationaddress.{name}");
    }
}
