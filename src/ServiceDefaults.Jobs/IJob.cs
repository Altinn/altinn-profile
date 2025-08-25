namespace Altinn.Authorization.ServiceDefaults.Jobs;

/// <summary>
/// A job that can be run on a schedule or on host lifecycle events.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Gets the name of the job.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Checks if the job should run at this time.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns><see langword="true"/>, if the job should be allowed to run at this time, otherwise <see langword="false"/>.</returns>
    public ValueTask<bool> ShouldRun(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the job.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    public Task RunAsync(CancellationToken cancellationToken = default);
}
