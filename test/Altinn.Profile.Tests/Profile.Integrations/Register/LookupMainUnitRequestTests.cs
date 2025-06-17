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
            var result = new LookupMainUnitRequest(orgNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"urn:altinn:organization:identifier-no:{orgNumber}", result.Data);
        }
    }
}
