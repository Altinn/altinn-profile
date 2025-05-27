using System.Diagnostics;

using static Altinn.Profile.Core.Telemetry.Telemetry.ContactRegistry;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for the contact register update job.
/// </summary>
partial class Telemetry
{
    private void InitContactRegisterUpdateJob(InitContext context)
    {
        InitMetricCounter(context, MetricNamePersonAdded, init: static m => m.Add(0));
        InitMetricCounter(context, MetricNamePersonUpdated, init: static m => m.Add(0));
    }

    /// <summary>
    /// Increments the counter for the number of persons created.
    /// </summary>
    public void PersonAdded() => _counters[MetricNamePersonAdded].Add(1);

    /// <summary>
    /// Increments the counter for the number of persons updated.
    /// </summary>
    public void PersonUpdated() => _counters[MetricNamePersonUpdated].Add(1);

    /// <summary>
    /// Starts a telemetry activity for the contact registry update job.
    /// </summary>
    /// <returns>The started activity.</returns>
    public Activity? StartContactRegistryUpdateJob()
    {
        var activity = ActivitySource.StartActivity($"{ActivityPrefix}.SyncContactInformationAsync");
        return activity;
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the contact register update job.
    /// </summary>
    internal static class ContactRegistry
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the contact registry.
        /// </summary>
        internal const string ActivityPrefix = "ContactRegistry";

        /// <summary>
        /// The name of the metric for the number of persons added through the sync job.
        /// </summary>
        internal static readonly string MetricNamePersonAdded = MetricName("person.added");

        /// <summary>
        /// The name of the metric for the number of persons updated through the sync job.
        /// </summary>
        internal static readonly string MetricNamePersonUpdated = MetricName("person.updated");

        private static string MetricName(string name) => Metrics.CreateName($"contactregistry.{name}");
    }
}
