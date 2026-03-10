using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Request model for the query resource for parties
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="QueryPartiesRequest"/> class.
    /// </remarks>
    /// <param name="identifiers">URN-formatted identifiers to query parties for</param>
    public class QueryPartiesRequest(string[] identifiers)
    {
        /// <summary>
        /// Data containing the urn values with identifiers for the parties to look up.
        /// </summary>
        [JsonPropertyName("data")]
        public string[] Data { get; init; } = identifiers;
    }
}
