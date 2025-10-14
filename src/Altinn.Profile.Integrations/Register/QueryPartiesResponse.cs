using System.Text.Json.Serialization;

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

        /// <summary>
        /// The party object
        /// </summary>
        public class Party
        {
            /// <summary>
            /// The party id.
            /// </summary>
            [JsonPropertyName("partyId")]
            public int PartyId { get; init; }

            /// <summary>
            /// The party uuid.
            /// </summary>
            [JsonPropertyName("partyUuid")]
            public Guid PartyUuid { get; init; }

            /// <summary>
            /// The organization identifier (org number).
            /// </summary>
            [JsonPropertyName("organizationIdentifier")]
            public string? OrganizationIdentifier { get; init; }
        }
    }
}
