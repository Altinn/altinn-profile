namespace Altinn.Profile.Core.PartyGroups;

/// <summary>
/// A group of parties organized by a user
/// </summary>
public record Group
{
    /// <summary>
    /// The group id
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// The group name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The id of the user owning the group
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Indicating whether or not he group is a favorite-group
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// A collection of parties in this group
    /// </summary>
    public List<PartyGroupAssociation>? Parties { get; set; }
}
