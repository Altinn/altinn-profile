#nullable disable

namespace Altinn.Profile.Core.PartyGroups;

/// <summary>
/// An association between a party and a group
/// </summary>
public record PartyGroupAssociation
{
    /// <summary>
    /// The id of the association
    /// </summary>
    public int AssociationId { get; set; }

    /// <summary>
    /// The group id
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// The Party's universally unique id
    /// </summary>
    public Guid PartyUuid { get; set; }

    /// <summary>
    /// The datetime when the association was created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The group the party is associated with
    /// </summary>
    public virtual Group Group { get; set; }
}
