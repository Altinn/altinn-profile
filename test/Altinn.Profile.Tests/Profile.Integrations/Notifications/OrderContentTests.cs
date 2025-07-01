using System;
using Altinn.Profile.Integrations.Notifications;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications
{
    public class OrderContentTests
    {
        [Theory]
        [InlineData("en", OrderContent.SmsEn)]
        [InlineData("nb", OrderContent.SmsNo)]
        [InlineData("nn", OrderContent.SmsNn)]
        [InlineData("se", OrderContent.SmsNo)] // Sami treated as Bokmål
        [InlineData("xx", OrderContent.SmsNo)] // Unknown treated as Bokmål
        [InlineData("", OrderContent.SmsNo)]
        [InlineData(null, OrderContent.SmsNo)]
        public void GetSmsContent_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = OrderContent.GetSmsContent(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", OrderContent.EmailSubjectEn)]
        [InlineData("nb", OrderContent.EmailSubjectNo)]
        [InlineData("nn", OrderContent.EmailSubjectNn)]
        [InlineData("se", OrderContent.EmailSubjectNo)] // Sami treated as Bokmål
        [InlineData("xx", OrderContent.EmailSubjectNo)] // Unknown treated as Bokmål
        [InlineData("", OrderContent.EmailSubjectNo)]
        [InlineData(null, OrderContent.EmailSubjectNo)]
        public void GetEmailSubject_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = OrderContent.GetEmailSubject(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "TestName", "Hi,<br /><br />You have changed your contact information for TestName with this email address. Please call us if that’s not correct. Tel: (+47) 75 00 60 00.<br /><br />You will receive notifications on new messages in Altinn. You can edit your notification settings under Profile at Altinn website .<br /><br />Best regards,<br />Altinn Support")]
        [InlineData("nb", "Navn", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Navn med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice")]
        [InlineData("nn", "Namn", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Namn med denne e-postadressa. Ring oss om dette ikkje stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldingar i Altinn. Du kan redigere varlingsinnstillingane dine under Profil på Altinn sine nettsider.<br /><br />Med venleg helsing<br />Altinn brukarservice")]
        [InlineData("se", "Sami", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Sami med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice")]
        [InlineData("xx", "Unknown", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Unknown med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice")]
        [InlineData("", "Empty", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Empty med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice")]
        [InlineData(null, "Null", "Hei.<br /><br />Du har oppdatert kontaktinformasjonen din for Null med denne e-postadressen. Ring oss om dette ikke stemmer. Tlf: 75 00 60 00.<br /><br />Du får varsling om nye meldinger i Altinn. Du kan redigere dine varslingsinnstillinger under Profil på Altinn sine nettsider.<br /><br />Med vennlig hilsen<br />Altinn Brukerservice")]
        public void GetEmailBody_ReturnsExpectedBody(string language, string reporteeName, string expected)
        {
            var result = OrderContent.GetEmailBody(language, reporteeName);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", OrderContent.EmailBodyTmpEn)]
        [InlineData("nb", OrderContent.EmailBodyTmpNo)]
        [InlineData("nn", OrderContent.EmailBodyTmpNn)]
        [InlineData("se", OrderContent.EmailBodyTmpNo)] // Sami treated as Bokmål
        [InlineData("xx", OrderContent.EmailBodyTmpNo)] // Unknown treated as Bokmål
        [InlineData("", OrderContent.EmailBodyTmpNo)]
        [InlineData(null, OrderContent.EmailBodyTmpNo)]
        public void GetTmpEmailBody_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = OrderContent.GetTmpEmailBody(language);
            Assert.Equal(expected, result);
        }
    }
}
