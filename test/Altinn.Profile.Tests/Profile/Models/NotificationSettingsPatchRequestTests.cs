using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Altinn.Profile.Core.Utils;
using Altinn.Profile.Models;
using Xunit;

namespace Altinn.Profile.Tests.Profile.ModelValidation
{
    public class NotificationSettingsPatchRequestTests
    {
        [Fact]
        public void Validate_WhenResourceIncludeListContainsInvalidUrn_ReturnsValidationError()
        {
            var model = new NotificationSettingsPatchRequest
            {
                ResourceIncludeList = new Optional<List<string>>(new() { "not-a-urn" }),
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Single(validationResults);
            Assert.Contains("ResourceIncludeList must contain valid URN values", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void Validate_WhenResourceIncludeListContainsDuplicates_ReturnsValidationError()
        {
            var model = new NotificationSettingsPatchRequest
            {
                ResourceIncludeList = new Optional<List<string>>(new() { "urn:altinn:resource:test_resource", "urn:altinn:resource:test_resource" }),
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Single(validationResults);
            Assert.Equal("ResourceIncludeList cannot contain duplicates", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void Validate_WhenEmailAndPhoneAreBothSetToNull_ReturnsValidationError()
        {
            var model = new NotificationSettingsPatchRequest
            {
                EmailAddress = new Optional<string>(null),
                PhoneNumber = new Optional<string>(null),
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Single(validationResults);
            Assert.Equal("The notification setting for a party must include either EmailAddress, PhoneNumber, or both.", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void Validate_WhenOnlyValidEmailIsSet_ReturnsNoValidationErrors()
        {
            var model = new NotificationSettingsPatchRequest
            {
                EmailAddress = new Optional<string>("test@test.com"),
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Empty(validationResults);
        }
    }
}
