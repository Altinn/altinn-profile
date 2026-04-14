using PhoneNumbers;

namespace Altinn.Profile.Validators
{
    /// <summary>
    /// Provides validation functionality for phone numbers.
    /// </summary>
    public static class PhoneNumberValidator
    {
        /// <summary>
        /// This is extra validation for phone numbers that cannot be validated with regex.
        /// </summary>
        public static bool IsValidPhoneNumber(string input)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            bool isValidNumber;

            try
            {
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(input, "NO");
                isValidNumber = phoneNumberUtil.IsValidNumber(phoneNumber);
            }
            catch (NumberParseException)
            {
                isValidNumber = false;
            }

            return isValidNumber;
        }
    }
}
