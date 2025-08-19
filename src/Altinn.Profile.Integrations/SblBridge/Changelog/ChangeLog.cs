namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Represents a container for a list of profile change log entries.
/// </summary>
public class ChangeLog
{
    /// <summary>
    /// Gets or sets the list of found log entries
    /// </summary>
    public List<ChangeLogItem> ProfileChangeLogBEList { get; set; } = [];

    /// <summary>
    /// Gets or sets the change log id to ask for next
    /// </summary>
    public int? NextChangeId { get; set; }

    /// <summary>
    /// Gets or sets the index of the element in the log list.
    /// </summary>
    public int? MinElementInLogList { get; set; }

    /// <summary>
    /// Gets or sets the index of the last element in the log list.
    /// </summary>
    public int? MaxElementInLogList { get; set; }
}
