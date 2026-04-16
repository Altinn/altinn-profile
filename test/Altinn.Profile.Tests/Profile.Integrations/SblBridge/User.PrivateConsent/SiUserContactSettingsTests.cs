using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.PrivateConsent;

public class SiUserContactSettingsTests
{
    public class TryFormatMobileNumberTests
    {
        [Fact]
        public void TryFormatMobileNumber_WhenNull_ReturnsTrueAndNull()
        {
            // Arrange
            string mobileNumber = null;

            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(mobileNumber, out var result);

            // Assert
            Assert.True(success);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void TryFormatMobileNumber_WhenWhitespace_ReturnsTrueAndOriginal(string mobileNumber)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(mobileNumber, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(mobileNumber, result);
        }

        [Theory]
        [InlineData("0047", "+47")]
        [InlineData("004712345678", "+4712345678")]
        [InlineData("0031612345678", "+31612345678")]
        [InlineData("001234567890", "+1234567890")]
        public void TryFormatMobileNumber_WhenStartsWith00_ReturnsTrueAndReplacesWithPlus(string input, string expected)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(input, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("12345678", "+4712345678")]
        [InlineData("98765432", "+4798765432")]
        [InlineData("40000000", "+4740000000")]
        [InlineData("99999999", "+4799999999")]
        public void TryFormatMobileNumber_When8DigitsWithoutPrefix_ReturnsTrueAndAddsNorwegianCountryCode(string input, string expected)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(input, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("+4712345678")]
        [InlineData("+31612345678")]
        [InlineData("+46701234567")]
        [InlineData("+1234567890")]
        [InlineData("+441234567890")]
        public void TryFormatMobileNumber_WhenAlreadyHasPlusPrefix_ReturnsTrueAndUnchanged(string mobileNumber)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(mobileNumber, out var result);

            // Assert
            Assert.True(success);
            Assert.Equal(mobileNumber, result);
        }

        [Theory]
        [InlineData("1234567")] // 7 digits
        [InlineData("123456789")] // 9 digits
        [InlineData("12345")] // 5 digits
        [InlineData("123")] // 3 digits
        public void TryFormatMobileNumber_WhenNotExactly8Digits_ReturnsFalseAndNull(string input)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(input, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("4712345678")] // 10 digits starting with 47
        [InlineData("474712345678")] // 12 digits starting with 47
        public void TryFormatMobileNumber_WhenStartsWith47ButNot8Digits_ReturnsFalseAndNull(string input)
        {
            // Act
            var success = SiUserContactSettings.TryFormatMobileNumber(input, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
