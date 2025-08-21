using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.ServiceDefaults.Jobs;

/// <summary>
/// Host lifecycle events that a job can be run at (in addition to the regular scheduled run).
/// </summary>
/// <remarks>
/// This is useful for jobs that for instance want the host to fail to start if the job fails during startup.
/// </remarks>
[Flags]
public enum JobHostLifecycles
{
    /// <summary>
    /// Do not run the job at any lifecycle events.
    /// </summary>
    None = 0,

    /// <summary>
    /// Run the job at the <see cref="IHostedLifecycleService.StartingAsync(CancellationToken)"/> point.
    /// </summary>
    Starting = 1 << 0,

    /// <summary>
    /// Run the job at the <see cref="IHostedService.StartAsync(CancellationToken)"/> point.
    /// </summary>
    Start = 1 << 1,

    /// <summary>
    /// Run the job at the <see cref="IHostedLifecycleService.StartedAsync(CancellationToken)"/> point.
    /// </summary>
    Started = 1 << 2,

    /// <summary>
    /// Run the job at the <see cref="IHostedLifecycleService.StoppingAsync(CancellationToken)"/> point.
    /// </summary>
    Stopping = 1 << 3,

    /// <summary>
    /// Run the job at the <see cref="IHostedService.StopAsync(CancellationToken)"/> point.
    /// </summary>
    Stop = 1 << 4,

    /// <summary>
    /// Run the job at the <see cref="IHostedLifecycleService.StoppedAsync(CancellationToken)"/> point.
    /// </summary>
    Stopped = 1 << 5,
}
