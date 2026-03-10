using Altinn.Profile.Integrations.Notifications;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications
{
    public class UserMessageBuilderTests
    {
        [Theory]
        [InlineData("en", UserMessageBuilder.InformSmsEn)]
        [InlineData("nb", UserMessageBuilder.InformSmsNo)]
        [InlineData("nn", UserMessageBuilder.InformSmsNn)]
        [InlineData("se", UserMessageBuilder.InformSmsNo)]
        [InlineData("xx", UserMessageBuilder.InformSmsNo)]
        [InlineData("", UserMessageBuilder.InformSmsNo)]
        [InlineData(null, UserMessageBuilder.InformSmsNo)]
        public void GetSmsContent_WithoutCode_ReturnsInformTemplate(string language, string expected)
        {
            var result = UserMessageBuilder.GetSmsContent(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "1234", UserMessageBuilder.VerificationCodeSmsEn)]
        [InlineData("nb", "0000", UserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("nn", "42", UserMessageBuilder.VerificationCodeSmsNn)]
        [InlineData("se", "abcd", UserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("xx", "z", UserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData("", "empty", UserMessageBuilder.VerificationCodeSmsNo)]
        [InlineData(null, "null", UserMessageBuilder.VerificationCodeSmsNo)]
        public void GetSmsContent_WithCode_ReturnsCodeTemplateWithSubstitution(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = UserMessageBuilder.GetSmsContent(language, code);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", UserMessageBuilder.EmailSubjectEn)]
        [InlineData("nb", UserMessageBuilder.EmailSubjectNo)]
        [InlineData("nn", UserMessageBuilder.EmailSubjectNn)]
        [InlineData("se", UserMessageBuilder.EmailSubjectNo)]
        [InlineData("xx", UserMessageBuilder.EmailSubjectNo)]
        [InlineData("", UserMessageBuilder.EmailSubjectNo)]
        [InlineData(null, UserMessageBuilder.EmailSubjectNo)]
        public void GetEmailSubject_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = UserMessageBuilder.GetEmailSubject(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", UserMessageBuilder.InformEmailBodyEn)]
        [InlineData("nb", UserMessageBuilder.InformEmailBodyNo)]
        [InlineData("nn", UserMessageBuilder.InformEmailBodyNn)]
        [InlineData("se", UserMessageBuilder.InformEmailBodyNo)]
        [InlineData("xx", UserMessageBuilder.InformEmailBodyNo)]
        [InlineData("", UserMessageBuilder.InformEmailBodyNo)]
        [InlineData(null, UserMessageBuilder.InformEmailBodyNo)]
        public void GetEmailBody_WithoutCode_ReturnsInformTemplate(string language, string expected)
        {
            var result = UserMessageBuilder.GetEmailBody(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "ABC", UserMessageBuilder.VerificationCodeEmailBodyEn)]
        [InlineData("nb", "NAVN", UserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("nn", "NAMN", UserMessageBuilder.VerificationCodeEmailBodyNn)]
        [InlineData("se", "SAMI", UserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("xx", "UNKNOWN", UserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData("", "EMPTY", UserMessageBuilder.VerificationCodeEmailBodyNo)]
        [InlineData(null, "NULL", UserMessageBuilder.VerificationCodeEmailBodyNo)]
        public void GetEmailBody_WithCode_ReturnsCodeTemplateWithSubstitution(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = UserMessageBuilder.GetEmailBody(language, code);
            Assert.Equal(expected, result);
        }
    }
}
