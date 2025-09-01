namespace Altinn.Authorization.ServiceDefaults.Jobs;

/// <summary>
/// Settings for a (potentially recurring) job.
/// </summary>
public interface IJobSettings
{
    /// <summary>
    /// Gets or sets the name of a lease that should be acquired before running the job.
    /// </summary>
    /// <remarks>
    /// If this is set, it will prevent the job from being run concurrently on multiple instances of the host at once.
    /// It will also modify the scheduling such that the interval is the time between the job finishes and the next job starts
    /// on any host instead of just on a single instance.
    /// </remarks>
    string? LeaseName { get; set; }

    /// <summary>
    /// Gets or sets the interval at which the job should run. Set to <see cref="TimeSpan.Zero"/> to disable running on an interval.
    /// </summary>
    TimeSpan Interval { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="JobHostLifecycles"/> that the job should run at.
    /// </summary>
    JobHostLifecycles RunAt { get; set; }

    /// <summary>
    /// Gets a collection of tags that can be used to categorize the job.
    /// </summary>
    ISet<string> Tags { get; }

    /// <summary>
    /// Gets or sets a delegate that will be called to check if the job is enabled or not.
    /// </summary>
    /// <remarks>
    /// This delegate will be called before each time the job is scheduled to run, and it runs in singleton scope.
    /// </remarks>
    Func<IServiceProvider, CancellationToken, ValueTask<bool>>? Enabled { get; set; }

    /// <summary>
    /// Gets or sets a delegate that will be called before the first job from a job registration is run.
    /// </summary>
    Func<IServiceProvider, CancellationToken, ValueTask>? WaitForReady { get; set; }
}
