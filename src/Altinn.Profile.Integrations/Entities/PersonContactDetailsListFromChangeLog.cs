using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a list of contact details for a person from the change log.
/// </summary>
public class PersonContactDetailsListFromChangeLog : IPersonContactDetailsListFromChangeLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonChangeLog"/> objects.</value>
    [JsonPropertyName("list")]
    public IEnumerable<PersonChangeLog>? ContactDetailsList { get; init; }
}

/// <summary>
/// Defines a list of contact details for a person from the change log.
/// </summary>
public interface IPersonContactDetailsListFromChangeLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonChangeLog"/> objects.</value>
    IEnumerable<PersonChangeLog>? ContactDetailsList { get; }
}
