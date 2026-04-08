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

        [Theory]
        [InlineData("12345")]
        [InlineData("abcdefg")]
        [InlineData("98765432")]
        [InlineData("004798765432")]
        public void Validate_WhenTypeIsSmsAndPhoneNumberHasWrongFormat_ReturnsValidationError(string phoneNumber)
        {
            var model = new AddressCodeSendRequest
            {
                Type = AddressType.Sms,
                Value = phoneNumber,
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();

            Assert.Contains(validationResults, r => r.ErrorMessage == "The field Value must match the regular expression '^(((\\+[0-9]{2})[0-9]+))$'.");
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
