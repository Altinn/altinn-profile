namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Defines a list of contact details for a person from the change log.
/// </summary>
public interface IPersonChangesLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonContactPreferencesSnapshot"/> objects.</value>
    IEnumerable<PersonContactPreferencesSnapshot>? ContactDetailsList { get; }
}
