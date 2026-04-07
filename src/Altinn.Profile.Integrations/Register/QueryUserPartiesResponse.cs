using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Response model for the lookup resource for parties
    /// </summary>
    public class QueryUserPartiesResponse
    {
        /// <summary>
        /// Data containing the party list.
        /// </summary>
        [JsonPropertyName("data")]
        public List<Altinn.Register.Contracts.Party> Data { get; init; } = [];
    }
}
