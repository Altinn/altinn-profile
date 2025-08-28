using System.Diagnostics;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for the contact register update job.
/// </summary>
partial class Telemetry
{
    /// <summary>
    /// Starts a telemetry activity for the contact registry update job.
    /// </summary>
    /// <returns>The started activity.</returns>
    public Activity? StartA2ImportJob(string jobName)
    {
        var activity = ActivitySource.StartActivity($"A2ImportJob.{jobName}");
        return activity;
    }
}
