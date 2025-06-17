using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Request model for the lookup resource for main units
    /// </summary>
    public class LookupMainUnitRequest
    {
        /// <summary>
        /// Data containing the urn of the organization with either orgNumber, partyId or PartyUuid.
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; init; }

        /// <summary>
        /// Set the OrgNumber in the Data property of the request.
        /// </summary>
        /// <param name="orgNumber">Organization Number of the organization to lookup parent units for</param>
        public static LookupMainUnitRequest Create(string orgNumber)
        {
            var request = new LookupMainUnitRequest
            {
                Data = $"urn:altinn:organization:identifier-no:{orgNumber}"
            };

            return request;
        }
    }
}
