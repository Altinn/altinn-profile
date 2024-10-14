using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

public interface IPersonContactDetailsListFromChangeLog
{
    IEnumerable<PersonNotificationStatusChangeLog>? ContactDetailsList { get; }
}

public class PersonContactDetailsListFromChangeLog : IPersonContactDetailsListFromChangeLog
{
    [JsonPropertyName("list")]
    public IEnumerable<PersonNotificationStatusChangeLog>? ContactDetailsList { get; init; }
}
