using System.ComponentModel.DataAnnotations;

using Altinn.Profile.Models;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Validators
{
    public class NotificationAddressRequestTests
    {
        [Fact]
        public void NotificationAddressRequest_WhenBothEmailAndPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { Email = "test@test.com", Phone = "98765432" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenNeitherEmailAndPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenEmailAndPhoneIsEmptyOrWhiteSpace_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { Email = string.Empty, Phone = " " };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenOnlyValidationEmailIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressRequest { Email = "test@test.com" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenOnlyValidPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { Phone = "98765432", Email = string.Empty };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenValidPhoneAndCountryCodeIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressRequest { Phone = "98765432", CountryCode = "+47" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenTooShortPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { CountryCode = "+47", Phone = "9876543" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("22345678")]
        [InlineData("32345678")]
        [InlineData("52345678")]
        [InlineData("62345678")]
        [InlineData("72345678")]
        [InlineData("82345678")]
        public void NotificationAddressRequest_WhenNorwegianPhoneNotLedBy4Or9_ReturnsValidationResults(string phone)
        {
            var model = new NotificationAddressRequest { CountryCode = "+47", Phone = phone };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Theory]
        [InlineData("+46", "798765432")] // Valid Swedish phone number
        [InlineData("+45", "81987654")] // Valid Danish phone number
        [InlineData("+49", "3098765432")] // Valid German phone number
        [InlineData("+48", "229876543")] // Valid Polish phone number
        [InlineData("+44", "07198765432")] // Valid UK phone number
        [InlineData("+1", "2125554567")] // Valid US phone number
        public void NotificationAddressRequest_WhenValidInternationalNumber_ReturnsNoValidationResults(string countryCode, string phone)
        {
            var model = new NotificationAddressRequest { CountryCode = countryCode, Phone = phone };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenValidInternationalPhoneIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressRequest { CountryCode = "+1", Phone = "2125551234", Email = string.Empty };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenOnlyCountryCodeIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { CountryCode = "+47" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressRequest_WhenCountryCodeAndEmailIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressRequest { CountryCode = "+47", Email = "test@test.com" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }
    }
}
