namespace Altinn.Authorization.ServiceDefaults.Jobs;

/// <summary>
/// Base class for jobs that can be run on a schedule or on host lifecycle events.
/// </summary>
public abstract class Job
    : IJob
{
    /// <inheritdoc/>
    string IJob.Name => GetType().Name;

    /// <inheritdoc/>
    ValueTask<bool> IJob.ShouldRun(CancellationToken cancellationToken)
        => ShouldRun(cancellationToken);

    /// <inheritdoc/>
    Task IJob.RunAsync(CancellationToken cancellationToken)
        => RunAsync(cancellationToken);

    /// <inheritdoc cref="IJob.ShouldRun(CancellationToken)"/>
    protected virtual ValueTask<bool> ShouldRun(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(true);

    /// <inheritdoc cref="IJob.RunAsync(CancellationToken)"/>
    protected abstract Task RunAsync(CancellationToken cancellationToken = default);
}
