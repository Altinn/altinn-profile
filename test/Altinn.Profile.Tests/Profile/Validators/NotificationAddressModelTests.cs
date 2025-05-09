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
            var model = new NotificationAddressModel { Phone = "98765432" };
            var validationContext = new ValidationContext(model);

            var validationResult = model.Validate(validationContext);

            Assert.Empty(validationResult);
        }
    }
}
