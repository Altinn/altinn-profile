namespace KofComparer.Models;

/// <summary>
/// The ways a registry id can differ between the KoFuVi dump and Profile.
/// </summary>
public enum DifferenceKind
{
    /// <summary>Active in KoFuVi, absent from Profile.</summary>
    MissingInProfile,

    /// <summary>Deleted in KoFuVi, still present in Profile.</summary>
    DeleteNotApplied,

    /// <summary>Active in both, but the last registry update differs.</summary>
    TimestampMismatch,

    /// <summary>Present in Profile with a registry update before the cutoff, absent from KoFuVi.</summary>
    ExtraInProfile,

    /// <summary>Present in Profile without a registry update time, absent from KoFuVi.</summary>
    ExtraInProfileWithoutTimestamp,
}

/// <summary>
/// A registry id that differs between the KoFuVi dump and Profile. Values from a side that has
/// no row for the id are null.
/// </summary>
public record RowDifference(
    DifferenceKind Kind,
    string RegistryID,
    bool? KoFuViIsDelete,
    DateTime? KoFuViUpdatedUtc,
    DateTime? ProfileUpdatedUtc)
{
    /// <summary>
    /// The timestamp used for sorting and grouping: KoFuVi's if known, otherwise Profile's.
    /// </summary>
    public DateTime? SortTimestamp => KoFuViUpdatedUtc ?? ProfileUpdatedUtc;
}
