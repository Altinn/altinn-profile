namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Defines a list of contact details for a person from the change log.
/// </summary>
public interface IPersonChangesLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonChangeLog"/> objects.</value>
    IEnumerable<PersonChangeLog>? ContactDetailsList { get; }
}
