using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Request model for the query resource for parties
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="QueryPartiesRequest"/> class.
    /// </remarks>
    /// <param name="orgNumbers">Organization Numbers to query parties for</param>
    public class QueryPartiesRequest(string[] orgNumbers)
    {
        /// <summary>
        /// Data containing the urn of the organization with the orgNumber.
        /// </summary>
        [JsonPropertyName("data")]
        public string[] Data { get; init; } = [.. orgNumbers.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => $"urn:altinn:organization:identifier-no:{o}")];
    }
}
