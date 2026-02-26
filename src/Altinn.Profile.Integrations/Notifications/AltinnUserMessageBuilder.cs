#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Provides localized message content for user notifications, including both
    /// address-change notifications and verification-code notifications.
    /// </summary>
    public static class AltinnUserMessageBuilder
    {
        // --- SMS templates: address-change notifications (informing the user their contact info was updated) ---

        /// <summary>English</summary>
        public const string InformSmsEn = "You have updated your Altinn contact information. Please call us if that's not correct - (+47) 75 00 60 00. You can edit this at Altinn website.";

        /// <summary>Norwegian Bokmål</summary>
        public const string InformSmsNo = "Kontaktinformasjonen din i Altinn er oppdatert. Ring oss om dette ikke stemmer - 75 00 60 00. Informasjonen kan redigeres på Altinn sine nettsider.";

        /// <summary>Norwegian Nynorsk</summary>
        public const string InformSmsNn = "Kontaktinformasjonen din i Altinn er oppdatert. Ring oss om dette ikkje stemmer - 75 00 60 00. Informasjonen kan redigerast på Altinn sine nettsider.";


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


        // --- Email body templates: address-change notifications (without reportee name) ---

        /// <summary>English email body for address-change notification (no reportee name).</summary>
        public const string InformEmailBodyEn = "Hi,<br /><br />You have changed your contact information with this email address. Please call us if that's not correct. Tel: (+47) 75 00 60 00.<br /><br />You will receive notifications on new messages in Altinn. You can edit your notification settings under Profile at Altinn website .<br /><br />Best regards,<br />Altinn Support";

        /// <summary>Norwegian Bokmål email body for address-change notification (no reportee name).</summary>
        public const string InformEmailBodyNo = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice";

        /// <summary>Norwegian Nynorsk email body for address-change notification (no reportee name).</summary>
        public const string InformEmailBodyNn = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din med denne e-postadressa. Ring oss om dette ikkje stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldingar i Altinn. Du kan redigere varlingsinnstillingane dine under Profil på Altinn sine nettsider.<br /><br />Med venleg helsing<br />Altinn brukarservice";


        // --- Email body templates: address-change notifications (with reportee name) ---

        /// <summary>English email body template with $reporteeName$ placeholder.</summary>
        public const string EmailBodyWithReporteeEn = "Hi,<br /><br />You have changed your contact information for $reporteeName$ with this email address. Please call us if that's not correct. Tel: (+47) 75 00 60 00.<br /><br />You will receive notifications on new messages in Altinn. You can edit your notification settings under Profile at Altinn website .<br /><br />Best regards,<br />Altinn Support";

        /// <summary>Norwegian Bokmål email body template with $reporteeName$ placeholder.</summary>
        public const string EmailBodyWithReporteeNo = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for $reporteeName$ med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice";

        /// <summary>Norwegian Nynorsk email body template with $reporteeName$ placeholder.</summary>
        public const string EmailBodyWithReporteeNn = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for $reporteeName$ med denne e-postadressa. Ring oss om dette ikkje stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldingar i Altinn. Du kan redigere varlingsinnstillingane dine under Profil på Altinn sine nettsider.<br /><br />Med venleg helsing<br />Altinn brukarservice";


        // --- Email body templates: verification-code notifications ---

        /// <summary>English email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyEn = "Enter the code $code$ to verify your email address in Altinn.";

        /// <summary>Norwegian Bokmål email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyNo = "Oppgi koden $code$ for å bekrefte e-postadressen din i Altinn.";

        /// <summary>Norwegian Nynorsk email body template for verification code. Contains $code$ placeholder.</summary>
        public const string VerificationCodeEmailBodyNn = "Skriv inn koden $code$ for å bekrefte e-postadressa di i Altinn";


        /// <summary>
        /// Gets the SMS content for the specified language. When a verification code is provided,
        /// returns the verification-code template with the code substituted; otherwise returns
        /// the address-change notification template.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <param name="verificationCode">Optional verification code to include in the message.</param>
        /// <returns>The localized SMS body text.</returns>
        public static string GetSmsContent(string language, string? verificationCode = null)
        {
            bool useCodeTemplate = verificationCode != null;
            var template = language switch
            {
                "en" => useCodeTemplate ? VerificationCodeSmsEn : InformSmsEn,
                "nb" => useCodeTemplate ? VerificationCodeSmsNo : InformSmsNo,
                "nn" => useCodeTemplate ? VerificationCodeSmsNn : InformSmsNn,
                "se" => useCodeTemplate ? VerificationCodeSmsNo : InformSmsNo,
                _ => useCodeTemplate ? VerificationCodeSmsNo : InformSmsNo,
            };

            return useCodeTemplate ? template.Replace("$code$", verificationCode) : template;
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
        /// Gets the email body for the specified language. When a verification code is provided,
        /// returns the verification-code template with the code substituted; otherwise returns
        /// the address-change notification template (without reportee name).
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <param name="verificationCode">Optional verification code to include in the message.</param>
        /// <returns>The localized email body text.</returns>
        public static string GetEmailBody(string language, string? verificationCode = null)
        {
            bool useCodeTemplate = verificationCode != null;
            var template = language switch
            {
                "en" => useCodeTemplate ? VerificationCodeEmailBodyEn : InformEmailBodyEn,
                "nb" => useCodeTemplate ? VerificationCodeEmailBodyNo : InformEmailBodyNo,
                "nn" => useCodeTemplate ? VerificationCodeEmailBodyNn : InformEmailBodyNn,
                "se" => useCodeTemplate ? VerificationCodeEmailBodyNo : InformEmailBodyNo,
                _ => useCodeTemplate ? VerificationCodeEmailBodyNo : InformEmailBodyNo,
            };

            return useCodeTemplate ? template.Replace("$code$", verificationCode) : template;
        }

        /// <summary>
        /// Gets the email body for the specified language, with the reportee name inserted.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", "nn", or "se").</param>
        /// <param name="reporteeName">The name to insert into the email body template.</param>
        /// <returns>The localized email body text with the reportee name inserted.</returns>
        public static string GetEmailBodyWithReportee(string language, string reporteeName)
        {
            string bodyTemplate = language switch
            {
                "en" => EmailBodyWithReporteeEn,
                "nb" => EmailBodyWithReporteeNo,
                "nn" => EmailBodyWithReporteeNn,
                "se" => EmailBodyWithReporteeNo,
                _ => EmailBodyWithReporteeNo,
            };

            return bodyTemplate.Replace("$reporteeName$", reporteeName);
        }
    }
}
