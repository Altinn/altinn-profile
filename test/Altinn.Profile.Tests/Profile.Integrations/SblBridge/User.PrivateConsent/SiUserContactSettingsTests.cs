using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.PrivateConsent;

public class SiUserContactSettingsTests
{
    public class FormatMobileNumberTests
    {
        [Fact]
        public void FormatMobileNumber_WhenNull_ReturnsNull()
        {
            // Arrange
            string mobileNumber = null;

            // Act
            var result = SiUserContactSettings.FormatMobileNumber(mobileNumber);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void FormatMobileNumber_WhenWhitespace_ReturnsOriginal(string mobileNumber)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(mobileNumber);

            // Assert
            Assert.Equal(mobileNumber, result);
        }

        [Theory]
        [InlineData("0047", "+47")]
        [InlineData("004712345678", "+4712345678")]
        [InlineData("0031612345678", "+31612345678")]
        [InlineData("001234567890", "+1234567890")]
        public void FormatMobileNumber_WhenStartsWith00_ReplacesWithPlus(string input, string expected)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("12345678", "+4712345678")]
        [InlineData("98765432", "+4798765432")]
        [InlineData("40000000", "+4740000000")]
        [InlineData("99999999", "+4799999999")]
        public void FormatMobileNumber_When8DigitsWithoutPrefix_AddsNorwegianCountryCode(string input, string expected)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("+4712345678")]
        [InlineData("+31612345678")]
        [InlineData("+46701234567")]
        [InlineData("+1234567890")]
        [InlineData("+441234567890")]
        public void FormatMobileNumber_WhenAlreadyHasPlusPrefix_ReturnsUnchanged(string mobileNumber)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(mobileNumber);

            // Assert
            Assert.Equal(mobileNumber, result);
        }

        [Theory]
        [InlineData("1234567", "1234567")] // 7 digits
        [InlineData("123456789", "123456789")] // 9 digits
        [InlineData("12345", "12345")] // 5 digits
        [InlineData("123", "123")] // 3 digits
        public void FormatMobileNumber_WhenNotExactly8Digits_ReturnsUnchanged(string input, string expected)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("4712345678", "4712345678")] // 10 digits starting with 47
        [InlineData("474712345678", "474712345678")] // 12 digits starting with 47
        public void FormatMobileNumber_WhenStartsWith47ButNot8Digits_ReturnsUnchanged(string input, string expected)
        {
            // Act
            var result = SiUserContactSettings.FormatMobileNumber(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
