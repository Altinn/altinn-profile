using System.ComponentModel.DataAnnotations;
using System.Linq;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Models;
using Xunit;

namespace Altinn.Profile.Tests.Profile.ModelValidation
{
    public class AddressCodeSendRequestTests
    {
        [Fact]
        public void Validate_WhenTypeIsEmailAndValueIsInvalid_ReturnsValidationError()
        {
            var model = new AddressCodeSendRequest
            {
                Type = AddressType.Email,
                Value = "invalid",
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.NotEmpty(validationResults);
        }

        [Fact]
        public void Validate_WhenTypeIsSmsAndPhoneNumberIsInvalid_ReturnsValidationError()
        {
            var model = new AddressCodeSendRequest
            {
                Type = AddressType.Sms,
                Value = "+4701234567",
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Contains(validationResults, r => r.ErrorMessage == "Phone number is not valid.");
        }

        [Fact]
        public void Validate_WhenTypeIsSmsAndPhoneNumberIsValid_ReturnsNoValidationErrors()
        {
            var model = new AddressCodeSendRequest
            {
                Type = AddressType.Sms,
                Value = "+4798765432",
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Empty(validationResults);
        }
    }
}
