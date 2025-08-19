using System.Diagnostics;

namespace Altinn.Profile.Tests.IntegrationTests.Mocks
{
    /// <summary>
    /// Entity representing a Role
    /// </summary>
    [DebuggerDisplay("{Value}", Name = "[{Type}]")]
    public record Role
    {
        /// <summary>
        /// Gets or sets the role type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the role
        /// </summary>
        public string Value { get; set; }
    }
}
