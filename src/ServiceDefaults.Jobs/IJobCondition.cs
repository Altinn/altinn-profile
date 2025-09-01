using System.Collections.Immutable;

namespace Altinn.Authorization.ServiceDefaults.Jobs;

/// <summary>
/// A condition that can be applied to one or more jobs, determining if they are allowed to run or not.
/// </summary>
public interface IJobCondition
{
    /// <summary>
    /// Gets the name of the job condition. Used for logging and diagnostics.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets tags this condition is applied to. If the list is empty, the condition applies to all jobs.
    /// </summary>
    public ImmutableArray<string> JobTags { get; }

    /// <summary>
    /// Checks if the job is allowed to run.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns><see langword="true"/>, if the job should be allowed to run at this time, otherwise <see langword="false"/>.</returns>
    public ValueTask<bool> ShouldRun(CancellationToken cancellationToken = default);
}
