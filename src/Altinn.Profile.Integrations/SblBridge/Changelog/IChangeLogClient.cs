namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Interface for fetching change log.
/// </summary>
public interface IChangeLogClient
{
    /// <summary>
    /// Fetches the profile change log starting from a given change id for the specified data type.
    /// </summary>
    /// <param name="changeDate">The change date to start from (exclusive per API contract).</param>
    /// <param name="dataType">The type of data to filter the change log by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The deserialized change log on success, or null if no content/non-success is handled as null.</returns>
    Task<ChangeLog?> GetChangeLog(DateTime changeDate, DataType dataType, CancellationToken cancellationToken);
}
