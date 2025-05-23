﻿using System.ComponentModel.DataAnnotations;
using Altinn.Profile.Models;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Validators
{
    public class NotificationAddressModelTests
    {
        [Fact]
        public void NotificationAddressModel_WhenBothEmailAndPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressModel { Email = "test@test.com", Phone = "98765432" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenNeitherEmailAndPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressModel { };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenEmailAndPhoneIsEmptyOrWhiteSpace_ReturnsValidationResults()
        {
            var model = new NotificationAddressModel { Email = string.Empty, Phone = " " };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenOnlyValidationEmailIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressModel { Email = "test@test.com" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenOnlyValidPhoneIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressModel { Phone = "98765432", Email = string.Empty };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenTooShortPhoneIsGiven_ReturnsValidationResults()
        {
            var model = new NotificationAddressModel { CountryCode = "+47", Phone = "9876543" };
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
        public void NotificationAddressModel_WhenNorwegianPhoneNotLedBy4Or9_ReturnsValidationResults(string phone)
        {
            var model = new NotificationAddressModel { CountryCode = "+47", Phone = phone };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.NotEmpty(validationResult);
        }

        [Fact]
        public void NotificationAddressModel_WhenValidInternationalPhoneIsGiven_ReturnsNoValidationResults()
        {
            var model = new NotificationAddressModel { CountryCode = "+1", Phone = "2125551234", Email = string.Empty };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }
    }
}
