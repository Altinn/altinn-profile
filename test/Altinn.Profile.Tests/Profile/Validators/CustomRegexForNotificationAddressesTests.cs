using Altinn.Profile.Models;
using Altinn.Profile.Validators;
using Microsoft.VisualBasic;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Validators
{
    public class CustomRegexForNotificationAddressesTests
    {
        [Theory]
        [InlineData("98765432")]
        [InlineData("9876543200")]
        public void CustomRegex_WhenPhone_IsValid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Phone");

            var validationResult = attribute.IsValid(input);

            Assert.True(validationResult);
        }

        [Theory]
        [InlineData("error")]
        [InlineData("+47987654321")]
        public void CustomRegex_WhenPhone_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Phone");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }

        [Theory]
        [InlineData("+47")]
        [InlineData("+385")]
        [InlineData("")]
        public void CustomRegex_WhenCountryCode_IsValid(string input)
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
        public void CustomRegex_WhenCountryCode_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("CountryCode");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }

        [Theory]
        [InlineData("test@test.com")]
        [InlineData("test-test@test.com")]
        [InlineData("test.test@test.com")]
        public void CustomRegex_WhenEmail_IsValid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Email");

            var validationResult = attribute.IsValid(input);

            Assert.True(validationResult);
        }

        [Theory]
        [InlineData("98765432")]
        [InlineData("test-test@@test.com")]
        [InlineData("test@test..com")]
        public void CustomRegex_WhenEmail_IsInvalid(string input)
        {
            var attribute = new CustomRegexForNotificationAddressesAttribute("Email");

            var validationResult = attribute.IsValid(input);

            Assert.False(validationResult);
        }
    }
}
