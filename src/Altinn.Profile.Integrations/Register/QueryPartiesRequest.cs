using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Request model for the lookup resource for main units
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LookupMainUnitRequest"/> class.
    /// </remarks>
    /// <param name="orgNumbers">Organization Number of the organization to lookup parent units for</param>
    public class QueryPartiesRequest(string[] orgNumbers)
    {
        /// <summary>
        /// Data containing the urn of the organization with either orgNumber, partyId or PartyUuid.
        /// </summary>
        [JsonPropertyName("data")]
        public string[] Data { get; init; } = [.. orgNumbers.Select(o => $"urn:altinn:organization:identifier-no:{o}")];
    }
}
