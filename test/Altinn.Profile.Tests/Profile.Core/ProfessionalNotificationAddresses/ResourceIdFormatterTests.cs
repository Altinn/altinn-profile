using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.ProfessionalNotificationAddresses
{
    public class ResourceIdFormatterTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("urn:altinn:resource:my-resource", "my-resource")]
        [InlineData("urn:altinn:resource:another-resource", "another-resource")]
        [InlineData("my-resource", "my-resource")]
        [InlineData("  urn:altinn:resource:trimmed  ", "trimmed")]
        [InlineData("  custom-resource  ", "custom-resource")]
        public void GetSanitizedResourceId_ReturnsExpected(string input, string expected)
        {
            var result = ResourceIdFormatter.GetSanitizedResourceId(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("my-resource", "urn:altinn:resource:my-resource")]
        [InlineData("urn:altinn:resource:already-prefixed", "urn:altinn:resource:already-prefixed")]
        [InlineData("  my-resource  ", "urn:altinn:resource:my-resource")]
        [InlineData("  urn:altinn:resource:trimmed  ", "urn:altinn:resource:trimmed")]
        public void AddPrefixToResourceId_ReturnsExpected(string input, string expected)
        {
            // Handle null input for AddPrefixToResourceId (method expects non-null, but trims and checks whitespace)
            var result = ResourceIdFormatter.AddPrefixToResourceId(input);
            Assert.Equal(expected, result);
        }
    }
}
