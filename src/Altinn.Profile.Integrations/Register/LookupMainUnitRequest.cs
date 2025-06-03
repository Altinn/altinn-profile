using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Request model for the lookup resource for main units
    /// </summary>
    public class LookupMainUnitRequest(string orgNumber)
    {
        /// <summary>
        /// Data containing the urn of the organization with either orgNumber, partyId or PartyUuid.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; init; } = $"urn:altinn:organization:identifier-no:{orgNumber}";
    }
}
