using System.Diagnostics;

using static Altinn.Profile.Core.Telemetry.Telemetry.ContactRegistry;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for the contact register update job.
/// </summary>
partial class Telemetry
{
    private void InitA2ImportJob(InitContext context)
    {
    }

    /// <summary>
    /// Starts a telemetry activity for the contact registry update job.
    /// </summary>
    /// <returns>The started activity.</returns>
    public Activity? StartA2ImportJob(string jobName)
    {
        var activity = ActivitySource.StartActivity($"{ActivityPrefix}.{jobName}");
        return activity;
    }

    /// <summary>
    /// This class holds a set of constants for the telemetry metrics of the contact register update job.
    /// </summary>
    internal static class ImportJob
    {
        /// <summary>
        /// The prefix for all telemetry activities related to the contact registry.
        /// </summary>
        internal const string ActivityPrefix = "A2ImportJob";
    }
}
