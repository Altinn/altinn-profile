using System.Text.Json.Serialization;

using Altinn.Profile.Core.Unit.ContactPoints;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Response model for the lookup resource for parties
    /// </summary>
    public class QueryPartiesResponse()
    {
        /// <summary>
        /// Data containing the party list.
        /// </summary>
        [JsonPropertyName("data")]
        public List<Party> Data { get; init; } = [];
    }
}
