using Altinn.Profile.Integrations.Notifications;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications
{
    public class AltinnUserMessageBuilderTests
    {
        [Theory]
        [InlineData("en", AltinnUserMessageBuilder.InformSmsEn)]
        [InlineData("nb", AltinnUserMessageBuilder.InformSmsNo)]
        [InlineData("nn", AltinnUserMessageBuilder.InformSmsNn)]
        [InlineData("se", AltinnUserMessageBuilder.InformSmsNo)]
        [InlineData("xx", AltinnUserMessageBuilder.InformSmsNo)]
        [InlineData("", AltinnUserMessageBuilder.InformSmsNo)]
        [InlineData(null, AltinnUserMessageBuilder.InformSmsNo)]
        public void GetSmsContent_WithoutCode_ReturnsInformTemplate(string language, string expected)
        {
            var result = AltinnUserMessageBuilder.GetSmsContent(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "1234", AltinnUserMessageBuilder.VerificationCodeSmsEn)]
        [InlineData("nb", "0000", AltinnUserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("nn", "42", AltinnUserMessageBuilder.VerificationCodeSmsNn)]
        [InlineData("se", "abcd", AltinnUserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("xx", "z", AltinnUserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("", "empty", AltinnUserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData(null, "null", AltinnUserMessageBuilder.VerificationCodeSmsNo)]
        public void GetSmsContent_WithCode_ReturnsCodeTemplateWithSubstitution(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = AltinnUserMessageBuilder.GetSmsContent(language, code);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", AltinnUserMessageBuilder.EmailSubjectEn)]
        [InlineData("nb", AltinnUserMessageBuilder.EmailSubjectNo)]
        [InlineData("nn", AltinnUserMessageBuilder.EmailSubjectNn)]
        [InlineData("se", AltinnUserMessageBuilder.EmailSubjectNo)]
        [InlineData("xx", AltinnUserMessageBuilder.EmailSubjectNo)]
        [InlineData("", AltinnUserMessageBuilder.EmailSubjectNo)]
        [InlineData(null, AltinnUserMessageBuilder.EmailSubjectNo)]
        public void GetEmailSubject_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = AltinnUserMessageBuilder.GetEmailSubject(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", AltinnUserMessageBuilder.InformEmailBodyEn)]
        [InlineData("nb", AltinnUserMessageBuilder.InformEmailBodyNo)]
        [InlineData("nn", AltinnUserMessageBuilder.InformEmailBodyNn)]
        [InlineData("se", AltinnUserMessageBuilder.InformEmailBodyNo)]
        [InlineData("xx", AltinnUserMessageBuilder.InformEmailBodyNo)]
        [InlineData("", AltinnUserMessageBuilder.InformEmailBodyNo)]
        [InlineData(null, AltinnUserMessageBuilder.InformEmailBodyNo)]
        public void GetEmailBody_WithoutCode_ReturnsInformTemplate(string language, string expected)
        {
            var result = AltinnUserMessageBuilder.GetEmailBody(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "ABC", AltinnUserMessageBuilder.VerificationCodeEmailBodyEn)]
        [InlineData("nb", "NAVN", AltinnUserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("nn", "NAMN", AltinnUserMessageBuilder.VerificationCodeEmailBodyNn)]
        [InlineData("se", "SAMI", AltinnUserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("xx", "UNKNOWN", AltinnUserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("", "EMPTY", AltinnUserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData(null, "NULL", AltinnUserMessageBuilder.VerificationCodeEmailBodyNo)]
        public void GetEmailBody_WithCode_ReturnsCodeTemplateWithSubstitution(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = AltinnUserMessageBuilder.GetEmailBody(language, code);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "TestName", "contact information")]
        [InlineData("nb", "Navn", "varslingsinnstillinger")]
        [InlineData("nn", "Namn", "varslingsinnstillingane")]
        [InlineData("se", "Sami", "varslingsinnstillinger")]
        [InlineData("xx", "Unknown", "varslingsinnstillinger")]
        [InlineData("", "Empty", "varslingsinnstillinger")]
        [InlineData(null, "Null", "varslingsinnstillinger")]
        public void GetEmailBodyWithReportee_ReturnsExpectedBody(string language, string reporteeName, string substring)
        {
            var result = AltinnUserMessageBuilder.GetEmailBodyWithReportee(language, reporteeName);
            Assert.Contains(reporteeName, result);
            Assert.Contains(substring, result);
        }
    }
}
