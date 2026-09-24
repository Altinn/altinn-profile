namespace KofComparer.Models;

/// <summary>
/// One row of the KoFuVi dump.
/// </summary>
public class KoFuViOrgAddress
{
    public required string RegistryID { get; set; } // First column

    public required bool IsDelete { get; set; } // Second column

    public required DateTime RegistryLastUpdatedUtc { get; set; } // Third column, converted from Norwegian local time

    /// <summary>
    /// True when the local timestamp fell in the repeated hour at the end of daylight saving
    /// time, so <see cref="RegistryLastUpdatedUtc"/> may be one hour off.
    /// </summary>
    public bool IsAmbiguousLocalTime { get; set; }
}
