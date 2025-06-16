using System;
using Altinn.Profile.Integrations.Register;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Register
{
    public class LookupMainUnitRequestTests
    {
        [Fact]
        public void Create_ValidOrgNumber_SetsDataCorrectly()
        {
            // Arrange
            var orgNumber = "123456789";

            // Act
            var result = LookupMainUnitRequest.Create(orgNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"urn:altinn:organization:identifier-no:{orgNumber}", result.Data);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_NullOrWhitespaceOrgNumber_ThrowsArgumentException(string? orgNumber)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => LookupMainUnitRequest.Create(orgNumber!));
            Assert.Equal("Organization number cannot be null or empty. (Parameter 'orgNumber')", ex.Message);
        }
    }
}
