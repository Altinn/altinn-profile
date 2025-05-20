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
        private const string _emailRegexPattern = @"((&quot;[^&quot;]+&quot;)|(([a-zA-Z0-9!#$%&amp;'*+\-=?\^_`{|}~])+(\.([a-zA-Z0-9!#$%&amp;'*+\-=?\^_`{|}~])+)*))@((((([a-zA-Z0-9æøåÆØÅ]([a-zA-Z0-9\-æøåÆØÅ]{0,61})[a-zA-Z0-9æøåÆØÅ]\.)|[a-zA-Z0-9æøåÆØÅ]\.){1,9})([a-zA-Z]{2,14}))|((\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})))";
        private const string _phoneRegexPattern = @"(([0-9]{5})|([0-9]{8})|((00[0-9]{2})[0-9]+)|((\+[0-9]{2})[0-9]+))$";
        private const string _countryCodeRegexPattern = @"(^\+([0-9]{1,3}))";

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
                "Email" => _emailRegexPattern,
                "Phone" => _phoneRegexPattern,
                "CountryCode" => _countryCodeRegexPattern,
                _ => string.Empty
            };
        }
    }
}
