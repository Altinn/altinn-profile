using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

public interface IPersonContactDetailsListFromChangeLog
{
    IEnumerable<IPersonNotificationStatusChangeLog>? ContactDetailsList { get; }
}

public class PersonContactDetailsListFromChangeLog : IPersonContactDetailsListFromChangeLog
{
    [JsonPropertyName("list")]
    public IEnumerable<IPersonNotificationStatusChangeLog>? ContactDetailsList { get; init; }
}
