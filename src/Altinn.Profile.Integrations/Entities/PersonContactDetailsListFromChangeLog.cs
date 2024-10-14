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
    /// <value>A collection of <see cref="PersonNotificationStatusChangeLog"/> objects.</value>
    [JsonPropertyName("list")]
    public IEnumerable<PersonNotificationStatusChangeLog>? ContactDetailsList { get; init; }
}

/// <summary>
/// Defines a list of contact details for a person from the change log.
/// </summary>
public interface IPersonContactDetailsListFromChangeLog
{
    /// <summary>
    /// Gets the list of contact details for the person.
    /// </summary>
    /// <value>A collection of <see cref="PersonNotificationStatusChangeLog"/> objects.</value>
    IEnumerable<PersonNotificationStatusChangeLog>? ContactDetailsList { get; }
}
