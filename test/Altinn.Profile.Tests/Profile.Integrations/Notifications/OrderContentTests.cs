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
        [InlineData("en", "TestName", "contact information")]
        [InlineData("nb", "Navn", "varslingsinnstillinger")]
        [InlineData("nn", "Namn", "varlingsinnstillingane")]
        [InlineData("se", "Sami", "varslingsinnstillinger")]
        [InlineData("xx", "Unknown", "varslingsinnstillinger")]
        [InlineData("", "Empty", "varslingsinnstillinger")]
        [InlineData(null, "Null", "varslingsinnstillinger")]
        public void GetEmailBody_ReturnsExpectedBody(string language, string reporteeName, string substring)
        {
            var result = OrderContent.GetEmailBody(language, reporteeName);
            Assert.Contains(reporteeName, result);
            Assert.Contains(substring, result);
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
