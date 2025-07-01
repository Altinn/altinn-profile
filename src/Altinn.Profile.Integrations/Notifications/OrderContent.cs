namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Provides localized content templates for notification messages (SMS and email) in Altinn.
    /// </summary>
    public static class OrderContent
    {
        /// <summary>
        /// English SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsEn = "You have updated your Altinn contact information. Please call us if that’s not correct - (+47) 75 00 60 00. You can edit this at Altinn website.";

        /// <summary>
        /// Norwegian Bokmål SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsNo = "Kontaktinformasjonen din i Altinn er oppdatert. Ring oss om dette ikke stemmer - 75 00 60 00. Informasjonen kan redigeres på Altinn sine nettsider.";

        /// <summary>
        /// Norwegian Nynorsk SMS message template for notifying users about updated contact information.
        /// </summary>
        public const string SmsNn = "Kontaktinformasjonen din i Altinn er oppdatert. Ring oss om dette ikkje stemmer - 75 00 60 00. Informasjonen kan redigerast på Altinn sine nettsider.";

        /// <summary>
        /// English email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectEn = "Your contact information in Altinn has been changed";

        /// <summary>
        /// English email body template for notifying users about changed contact information, with reportee name.
        /// </summary>
        public const string EmailBodyEn = "Hi,<br /><br />You have changed your contact information for $reporteeName$ with this email address. Please call us if that’s not correct. Tel: (+47) 75 00 60 00.<br /><br />You will receive notifications on new messages in Altinn. You can edit your notification settings under Profile at Altinn website .<br /><br />Best regards,<br />Altinn Support";

        /// <summary>
        /// English email body template for notifying users about changed contact information, without reportee name.
        /// </summary>
        public const string EmailBodyTmpEn = "Hi,<br /><br />You have changed your contact information with this email address. Please call us if that’s not correct. Tel: (+47) 75 00 60 00.<br /><br />You will receive notifications on new messages in Altinn. You can edit your notification settings under Profile at Altinn website .<br /><br />Best regards,<br />Altinn Support";

        /// <summary>
        /// Norwegian Bokmål email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectNo = "Din kontaktinformasjon i Altinn er endret";

        /// <summary>
        /// Norwegian Bokmål email body template for notifying users about changed contact information, with reportee name.
        /// </summary>
        public const string EmailBodyNo = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for $reporteeName$ med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice";

        /// <summary>
        /// Norwegian Bokmål email body template for notifying users about changed contact information, without reportee name.
        /// </summary>
        public const string EmailBodyTmpNo = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice";

        /// <summary>
        /// Norwegian Nynorsk email subject for notifying users about changed contact information.
        /// </summary>
        public const string EmailSubjectNn = "Kontaktinformasjonen din i Altinn er endra";

        /// <summary>
        /// Norwegian Nynorsk email body template for notifying users about changed contact information, with reportee name.
        /// </summary>
        public const string EmailBodyNn = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for $reporteeName$ med denne e-postadressa. Ring oss om dette ikkje stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldingar i Altinn. Du kan redigere varlingsinnstillingane dine under Profil på Altinn sine nettsider.<br /><br />Med venleg helsing<br />Altinn brukarservice";

        /// <summary>
        /// Norwegian Nynorsk email body template for notifying users about changed contact information, without reportee name.
        /// </summary>
        public const string EmailBodyTmpNn = "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din med denne e-postadressa. Ring oss om dette ikkje stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldingar i Altinn. Du kan redigere varlingsinnstillingane dine under Profil på Altinn sine nettsider.<br /><br />Med venleg helsing<br />Altinn brukarservice";

        /// <summary>
        /// Gets the SMS content template for the specified language.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <returns>The SMS message template in the specified language.</returns>
        public static string GetSmsContent(string language)
        {
            return language switch
            {
                "en" => OrderContent.SmsEn,
                "nb" => OrderContent.SmsNo,
                "nn" => OrderContent.SmsNn,
                "se" => OrderContent.SmsNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContent.SmsNo,
            };
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
                "en" => OrderContent.EmailSubjectEn,
                "nb" => OrderContent.EmailSubjectNo,
                "nn" => OrderContent.EmailSubjectNn,
                "se" => OrderContent.EmailSubjectNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContent.EmailSubjectNo,
            };
        }

        /// <summary>
        /// Gets the email body template for the specified language, replacing the reportee name placeholder. Not to be used yet.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <param name="reporteeName">The name to insert into the email body template.</param>
        /// <returns>The email body in the specified language with the reportee name inserted.</returns>
        public static string GetEmailBody(string language, string reporteeName)
        {
            string bodyTemplate = language switch
            {
                "en" => OrderContent.EmailBodyEn,
                "nb" => OrderContent.EmailBodyNo,
                "nn" => OrderContent.EmailBodyNn,
                "se" => OrderContent.EmailBodyNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContent.EmailBodyNo,
            };
            return bodyTemplate.Replace("$reporteeName$", reporteeName);
        }

        /// <summary>
        /// Gets the temporary email body template for the specified language, without reportee name.
        /// </summary>
        /// <param name="language">The language code ("en", "nb", or "nn").</param>
        /// <returns>The temporary email body in the specified language.</returns>
        public static string GetTmpEmailBody(string language)
        {
            return language switch
            {
                "en" => OrderContent.EmailBodyTmpEn,
                "nb" => OrderContent.EmailBodyTmpNo,
                "nn" => OrderContent.EmailBodyTmpNn,
                "se" => OrderContent.EmailBodyTmpNo, // Sami is treated as Norwegian Bokmål
                _ => OrderContent.EmailBodyTmpNo,
            };
        }
    }
}
