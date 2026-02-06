namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Provides localized content templates for notification messages (SMS and email) in Altinn.
    /// </summary>
    public static class OrderContentWithCode
    {
        /// <summary>
        /// English SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsEn = "Enter the code $code$ to verify your phone number.";

        /// <summary>
        /// Norwegian Bokmål SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsNo = "Oppgi koden $code$ for å bekrefte telefonnummeret ditt.";

        /// <summary>
        /// Norwegian Nynorsk SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsNn = "Oppgje koden $code$ for å stadfeste telefonnummeret ditt.";

        /// <summary>
        /// English email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectEn = "Your contact information in Altinn has been changed";

        /// <summary>
        /// English email body template for notifying users about changed contact information.
        /// </summary>
        public const string EmailBodyEn = "Enter the code $code$ to verify your email address.";

        /// <summary>
        /// Norwegian Bokmål email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectNo = "Din kontaktinformasjon i Altinn er endret";

        /// <summary>
        /// Norwegian Bokmål email body template for notifying users about changed contact information.
        /// </summary>
        public const string EmailBodyNo = "Oppgi koden $code$ for å bekrefte e-postadressen din.";

        /// <summary>
        /// Norwegian Nynorsk email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectNn = "Kontaktinformasjonen din i Altinn er endra";

        /// <summary>
        /// Norwegian Nynorsk email body template for notifying users about changed contact information.
        /// </summary>
        public const string EmailBodyNn = "Oppgje koden $code$ for å stadfeste denne e-postadressa.";

        /// <summary>
        /// Gets the SMS content template for the specified language.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <param name="code">The verification code to insert into the email body template.</param>
        /// <returns>The SMS message template in the specified language.</returns>
        public static string GetSmsContent(string language, string code)
        {
            var bodyTemplate = language switch
            {
                "en" => OrderContentWithCode.SmsEn,
                "nb" => OrderContentWithCode.SmsNo,
                "nn" => OrderContentWithCode.SmsNn,
                "se" => OrderContentWithCode.SmsNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContentWithCode.SmsNo,
            };
            return bodyTemplate.Replace("$code$", code);
        }

        /// <summary>
        /// Gets the email subject template for the specified language.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <returns>The email subject in the specified language.</returns>
        public static string GetEmailSubject(string language)
        {
            return language switch
            {
                "en" => OrderContentWithCode.EmailSubjectEn,
                "nb" => OrderContentWithCode.EmailSubjectNo,
                "nn" => OrderContentWithCode.EmailSubjectNn,
                "se" => OrderContentWithCode.EmailSubjectNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContentWithCode.EmailSubjectNo,
            };
        }

        /// <summary>
        /// Gets the email body template for the specified language, replacing the reportee name placeholder. Not to be used yet.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <param name="code">The verification code to insert into the email body template.</param>
        /// <returns>The email body in the specified language with the reportee name inserted.</returns>
        public static string GetEmailBody(string language, string code)
        {
            string bodyTemplate = language switch
            {
                "en" => OrderContentWithCode.EmailBodyEn,
                "nb" => OrderContentWithCode.EmailBodyNo,
                "nn" => OrderContentWithCode.EmailBodyNn,
                "se" => OrderContentWithCode.EmailBodyNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContentWithCode.EmailBodyNo,
            };
            return bodyTemplate.Replace("$code$", code);
        }
    }
}
