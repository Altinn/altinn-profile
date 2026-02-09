using Altinn.Profile.Integrations.Notifications;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications
{
    public class OrderContentWithCodeTests
    {
        [Theory]
        [InlineData("en", "1234", OrderContentWithCode.SmsEn)]
        [InlineData("nb", "0000", OrderContentWithCode.SmsNo)]
        [InlineData("nn", "42", OrderContentWithCode.SmsNn)]
        [InlineData("se", "abcd", OrderContentWithCode.SmsNo)] // Sami treated as Bokmål
        [InlineData("xx", "z", OrderContentWithCode.SmsNo)] // Unknown treated as Bokmål
        [InlineData("", "empty", OrderContentWithCode.SmsNo)]
        [InlineData(null, "null", OrderContentWithCode.SmsNo)]
        public void GetSmsContent_ReturnsExpectedTemplateWithCode(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = OrderContentWithCode.GetSmsContent(language, code);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", OrderContentWithCode.EmailSubjectEn)]
        [InlineData("nb", OrderContentWithCode.EmailSubjectNo)]
        [InlineData("nn", OrderContentWithCode.EmailSubjectNn)]
        [InlineData("se", OrderContentWithCode.EmailSubjectNo)] // Sami treated as Bokmål
        [InlineData("xx", OrderContentWithCode.EmailSubjectNo)] // Unknown treated as Bokmål
        [InlineData("", OrderContentWithCode.EmailSubjectNo)]
        [InlineData(null, OrderContentWithCode.EmailSubjectNo)]
        public void GetEmailSubject_ReturnsExpectedTemplate(string language, string expected)
        {
            var result = OrderContentWithCode.GetEmailSubject(language);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("en", "ABC", OrderContentWithCode.EmailBodyEn)]
        [InlineData("nb", "NAVN", OrderContentWithCode.EmailBodyNo)]
        [InlineData("nn", "NAMN", OrderContentWithCode.EmailBodyNn)]
        [InlineData("se", "SAMI", OrderContentWithCode.EmailBodyNo)] // Sami treated as Bokmål
        [InlineData("xx", "UNKNOWN", OrderContentWithCode.EmailBodyNo)] // Unknown treated as Bokmål
        [InlineData("", "EMPTY", OrderContentWithCode.EmailBodyNo)]
        [InlineData(null, "NULL", OrderContentWithCode.EmailBodyNo)]
        public void GetEmailBody_ReturnsExpectedTemplateWithCode(string language, string code, string template)
        {
            var expected = template.Replace("$code$", code);
            var result = OrderContentWithCode.GetEmailBody(language, code);
            Assert.Equal(expected, result);
        }
    }
}
