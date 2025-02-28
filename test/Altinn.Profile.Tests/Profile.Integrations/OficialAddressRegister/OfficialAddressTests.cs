using System;
using Altinn.Profile.Integrations.OfficialAddressRegister;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OficialAddressRegister
{
    public class OfficialAddressTests
    {
        [Fact]
        public void ThrowsWhenMissingContent()
        {
            var officialAddress = new OfficialAddress();

            Assert.Throws<ArgumentNullException>(() => officialAddress.Content);
        }

        [Fact]
        public void DeserializesStringifiedEmailContent()
        {
            var officialAddress = new OfficialAddress() { ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"epostadresse\":{\"navn\":\"test@test.no\",\"domenenavn\":\"test.no\",\"brukernavn\":\"test\"}},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
            var content = officialAddress.Content;

            Assert.Equal("27ae0c8bea1f4f02a974c10429c32758", content.ContactPoint.Id);

            Assert.Equal("test@test.no", content.ContactPoint.DigitalContactPoint.EmailAddress.Name);
            Assert.Equal("test", content.ContactPoint.DigitalContactPoint.EmailAddress.Username);
            Assert.Equal("test.no", content.ContactPoint.DigitalContactPoint.EmailAddress.Domain);

            Assert.Null(content.ContactPoint.DigitalContactPoint.PhoneNumber);

            Assert.Equal("920212345", content.ContactPoint.UnitContactInfo.UnitIdentifier.Value);
            Assert.Equal("ORGANISASJONSNUMMER", content.ContactPoint.UnitContactInfo.UnitIdentifier.Type);
        }

        [Fact]
        public void DeserializesStringifiedPhoneContent()
        {
            var officialAddress = new OfficialAddress() { ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"mobiltelefon\":{\"navn\":\"4798765432\",\"internasjonaltPrefiks\":\"47\",\"nasjonaltNummer\":\"98765432\"}},\"identifikator\":\"37ab4733648c4d5b825a813c6e1ace70\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920254321\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
            var content = officialAddress.Content;

            Assert.Equal("37ab4733648c4d5b825a813c6e1ace70", content.ContactPoint.Id);

            Assert.Equal("98765432", content.ContactPoint.DigitalContactPoint.PhoneNumber.NationalNumber);
            Assert.Equal("4798765432", content.ContactPoint.DigitalContactPoint.PhoneNumber.Number);
            Assert.Equal("47", content.ContactPoint.DigitalContactPoint.PhoneNumber.Domain);

            Assert.Null(content.ContactPoint.DigitalContactPoint.EmailAddress);

            Assert.Equal("920254321", content.ContactPoint.UnitContactInfo.UnitIdentifier.Value);
            Assert.Equal("ORGANISASJONSNUMMER", content.ContactPoint.UnitContactInfo.UnitIdentifier.Type);
        }
    }
}
