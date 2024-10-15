using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a list of contact details for a person from the change log.
/// </summary>
public class PersonContactPreferencesChangesLog : IPersonContactPreferencesChangesLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonContactPreferencesSnapshot"/> objects.</value>
    [JsonPropertyName("list")]
    public IEnumerable<PersonContactPreferencesSnapshot>? ContactDetailsList { get; init; }
}
