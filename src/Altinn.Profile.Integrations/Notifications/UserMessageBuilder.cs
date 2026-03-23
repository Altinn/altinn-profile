#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Provides localized message content for verification-code notifications.
    /// </summary>
    public static class UserMessageBuilder
    {
        // --- SMS templates: verification-code notifications ---

        /// <summary>English SMS template for sending a verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeSmsEn = "Enter the code $code$ to verify your phone number in Altinn.";

        /// <summary>Norwegian Bokmål SMS template for sending a verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeSmsNo = "Oppgi koden $code$ for å bekrefte telefonnummeret ditt i Altinn.";

        /// <summary>Norwegian Nynorsk SMS template for sending a verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeSmsNn = "Skriv inn koden $code$ for å bekrefte telefonnummeret ditt i Altinn.";


        // --- Email subject (shared across notification types) ---

        /// <summary>English email subject for contact information change.</summary>
        public const string EmailSubjectEn = "Your contact information in Altinn has been changed";

        /// <summary>Norwegian Bokmål email subject for contact information change.</summary>
        public const string EmailSubjectNo = "Kontaktinformasjonen din i Altinn er endret";

        /// <summary>Norwegian Nynorsk email subject for contact information change.</summary>
        public const string EmailSubjectNn = "Kontaktinformasjonen din i Altinn er endra";

        // --- Email body templates: verification-code notifications ---

        /// <summary>English email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyEn = "Enter the code $code$ to verify your email address in Altinn.";

        /// <summary>Norwegian Bokmål email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyNo = "Oppgi koden $code$ for å bekrefte e-postadressen din i Altinn.";

        /// <summary>Norwegian Nynorsk email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyNn = "Skriv inn koden $code$ for å bekrefte e-postadressa di i Altinn.";


        /// <summary>
        /// Gets the SMS content for the specified language with the verification code substituted.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <param name="verificationCode">Verification code to include in the message.</param>
        /// <returns>The localized SMS body text.</returns>
        public static string GetSmsContent(string language, string verificationCode)
        {
            var template = language switch
            {
                "en" => VerificationCodeSmsEn,
                "nb" => VerificationCodeSmsNo,
                "nn" => VerificationCodeSmsNn,
                "se" => VerificationCodeSmsNo,
                _ => VerificationCodeSmsNo,
            };

            return template.Replace("$code$", verificationCode);
        }

        /// <summary>
        /// Gets the email subject for the specified language.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <returns>The localized email subject.</returns>
        public static string GetEmailSubject(string language)
        {
            return language switch
            {
                "en" => EmailSubjectEn,
                "nb" => EmailSubjectNo,
                "nn" => EmailSubjectNn,
                "se" => EmailSubjectNo,
                _ => EmailSubjectNo,
            };
        }

        /// <summary>
        /// Gets the SMS content for the specified language with the verification code substituted.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <param name="verificationCode">Verification code to include in the message.</param>
        /// <returns>The localized email body text.</returns>
        public static string GetEmailBody(string language, string verificationCode)
        {
            var template = language switch
            {
                "en" => VerificationCodeEmailBodyEn,
                "nb" => VerificationCodeEmailBodyNo,
                "nn" => VerificationCodeEmailBodyNn,
                "se" => VerificationCodeEmailBodyNo,
                _ => VerificationCodeEmailBodyNo,
            };

            return template.Replace("$code$", verificationCode);
        }
    }
}
