using System;
using System.ComponentModel.DataAnnotations;

namespace Altinn.Profile.Validators
{
    /// <summary>
    /// Custom regex attribute for notification address validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomRegexForNotificationAddressesAttribute : RegularExpressionAttribute
    {
        // Professional (personal) notification addresses
        private const string _emailRegexPattern = @"^((""[^""]+"")|(([a-zA-Z0-9!#$%&'*+\-=?\^_`{|}~])+(\.([a-zA-Z0-9!#$%&'*+\-=?\^_`{|}~])+)*))@((((([a-zA-Z0-9æøåÆØÅ]([a-zA-Z0-9\-æøåÆØÅ]{0,61})[a-zA-Z0-9æøåÆØÅ]\.)|[a-zA-Z0-9æøåÆØÅ]\.){1,9})([a-zA-Z]{2,14}))|((\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})))$";
        private const string _phoneRegexPattern = @"^(([0-9]{5})|([0-9]{8})|((00[0-9]{2})[0-9]+)|((\+[0-9]{2})[0-9]+))$";
        private const string _internationalPhoneRegexPattern = @"^(((\+[0-9]{2})[0-9]+))$";

        // Organization notification addresses (KoFuVi)
        private const string _kofEmailRegexPattern = @"^((([a-zA-Z0-9!#$%&'*+\-=?\^_`{}~])+(\.([a-zA-Z0-9!#$%&'*+\-=?\^_`{}~])+)*)@(((([a-zA-Z0-9æøåÆØÅ]([a-zA-Z0-9\-æøåÆØÅ]{0,61})[a-zA-Z0-9æøåÆØÅ]\.)|[a-zA-Z0-9æøåÆØÅ]\.){1,9})([a-zA-Z]{2,14})))$";
        private const string _kofPhoneRegexPattern = @"(^[0-9]+$)";
        private const string _kofCountryCodeRegexPattern = @"(^\+([0-9]{1,3}))";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegexForNotificationAddressesAttribute"/> class
        /// </summary>
        /// <param name="inputType">the type of input</param>
        public CustomRegexForNotificationAddressesAttribute(string inputType)
            : base(GetRegex(inputType))
        {
        }

        private static string GetRegex(string inputType)
        {
            return inputType switch
            {
                ValidationRule.EmailAddress => _emailRegexPattern,
                ValidationRule.PhoneNumber => _phoneRegexPattern,
                ValidationRule.InternationalPhoneNumber => _internationalPhoneRegexPattern,
                ValidationRule.OrganizationEmail => _kofEmailRegexPattern,
                ValidationRule.OrganizationPhone => _kofPhoneRegexPattern,
                ValidationRule.OrganizationCountryCode => _kofCountryCodeRegexPattern,
                _ => throw new ArgumentException($"Unknown input type: {inputType}", nameof(inputType)),
            };
        }
    }

    /// <summary>
    /// Constants for validation rule types used with CustomRegexForNotificationAddressesAttribute
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// Professional email address validation rule
        /// </summary>
        public const string EmailAddress = "EmailAddress";

        /// <summary>
        /// Professional phone number validation rule
        /// </summary>
        public const string PhoneNumber = "PhoneNumber";

        /// <summary>
        /// Professional phone number with country code validation rule
        /// </summary>
        public const string InternationalPhoneNumber = "InternationalPhoneNumber";

        /// <summary>
        /// Organization email address validation rule
        /// </summary>
        public const string OrganizationEmail = "OrganizationEmail";

        /// <summary>
        /// Organization phone number validation rule
        /// </summary>
        public const string OrganizationPhone = "OrganizationPhone";

        /// <summary>
        /// Country code validation rule
        /// </summary>
        public const string OrganizationCountryCode = "OrganizationCountryCode";
    }
}
