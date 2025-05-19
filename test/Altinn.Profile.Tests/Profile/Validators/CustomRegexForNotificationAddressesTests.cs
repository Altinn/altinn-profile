using Altinn.Profile.Validators;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Validators
{
    public class CustomRegexForNotificationAddressesTests
    {
        [Theory]
        [InlineData("98765432")]
        [InlineData("98765")]
        [InlineData("")]
        [InlineData(null)]
        public void CustomRegex_WhenPhoneHasAllowedValues_ReturnsValidResult(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Phone");

            var validationResult = attribute.IsValid(input);

            Assert.True(validationResult);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("error")]
        [InlineData("+47")]
        public void CustomRegex_WhenPhoneHasInvalidValues_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Phone");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }

        [Theory]
        [InlineData("+47")]
        [InlineData("+385")]
        [InlineData("")]
        [InlineData(null)]
        public void CustomRegex_WhenCountryCodeHasAllowedValues_IsValid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("CountryCode");

            var validationResult = attribute.IsValid(input);

            Assert.True(validationResult);
        }

        [Theory]
        [InlineData("++47")]
        [InlineData("47")]
        [InlineData("+4777")]
        [InlineData("0047")]
        public void CustomRegex_WhenCountryCodeHasWrongFormat_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("CountryCode");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }

        [Theory]
        [InlineData("test@test.com")]
        [InlineData("test-test@test.com")]
        [InlineData("test.test@test.com")]
        [InlineData("")]
        [InlineData(null)]
        public void CustomRegex_WhenEmailHasAllowedValues_IsValid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Email");

            var validationResult = attribute.IsValid(input);

            Assert.True(validationResult);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("98765432")]
        [InlineData("test-test@@test.com")]
        [InlineData("test@test..com")]
        public void CustomRegex_WhenEmailHasWrongFormat_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Email");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }
    }
}
