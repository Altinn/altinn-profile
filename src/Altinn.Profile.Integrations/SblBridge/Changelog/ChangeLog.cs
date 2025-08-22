namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Represents a container for a list of profile change log entries.
/// </summary>
public class ChangeLog
{
    /// <summary>
    /// Gets or sets the list of found log entries
    /// </summary>
    public List<ChangeLogItem> ProfileChangeLogList { get; set; } = [];
}
