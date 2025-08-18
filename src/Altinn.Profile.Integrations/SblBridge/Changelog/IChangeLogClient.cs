namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Interface for fetching change log.
/// </summary>
public interface IChangeLogClient
{
    /// <summary>
    /// Updates the user's favorites based on the provided request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<ChangeLog?> GetChangeLog(int changeId, DataType dataType);
}
