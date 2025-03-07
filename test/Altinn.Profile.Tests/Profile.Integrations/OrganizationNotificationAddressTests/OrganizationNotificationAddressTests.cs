using System;
using Altinn.Profile.Integrations.OrganizationNotificationAddress;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class OrganizationNotificationAddressTests
{
    [Fact]
    public void Content_WhenMissingContentString_ReturnsNull()
    {
        var organizationNotificationAddress = new OrganizationNotificationAddress();

        Assert.Null(organizationNotificationAddress.Content);
    }

    [Fact]
    public void Content_DeserializesStringifiedEmail()
    {
        var organizationNotificationAddress = new OrganizationNotificationAddress() { ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"epostadresse\":{\"navn\":\"test@test.no\",\"domenenavn\":\"test.no\",\"brukernavn\":\"test\"}},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var content = organizationNotificationAddress.Content;

        Assert.Equal("27ae0c8bea1f4f02a974c10429c32758", content.ContactPoint.Id);

        Assert.Equal("test@test.no", content.ContactPoint.DigitalContactPoint.EmailAddress.Name);
        Assert.Equal("test", content.ContactPoint.DigitalContactPoint.EmailAddress.Username);
        Assert.Equal("test.no", content.ContactPoint.DigitalContactPoint.EmailAddress.Domain);

        Assert.Null(content.ContactPoint.DigitalContactPoint.PhoneNumber);

        Assert.Equal("920212345", content.ContactPoint.UnitContactInfo.UnitIdentifier.Value);
        Assert.Equal("ORGANISASJONSNUMMER", content.ContactPoint.UnitContactInfo.UnitIdentifier.Type);
    }

    [Fact]
    public void Content_DeserializesStringifiedPhoneContent()
    {
        var organizationNotificationAddress = new OrganizationNotificationAddress() { ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"mobiltelefon\":{\"navn\":\"4798765432\",\"internasjonaltPrefiks\":\"47\",\"nasjonaltNummer\":\"98765432\"}},\"identifikator\":\"37ab4733648c4d5b825a813c6e1ace70\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920254321\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var content = organizationNotificationAddress.Content;

        Assert.Equal("37ab4733648c4d5b825a813c6e1ace70", content.ContactPoint.Id);

        Assert.Equal("98765432", content.ContactPoint.DigitalContactPoint.PhoneNumber.NationalNumber);
        Assert.Equal("4798765432", content.ContactPoint.DigitalContactPoint.PhoneNumber.Number);
        Assert.Equal("47", content.ContactPoint.DigitalContactPoint.PhoneNumber.Prefix);

        Assert.Null(content.ContactPoint.DigitalContactPoint.EmailAddress);

        Assert.Equal("920254321", content.ContactPoint.UnitContactInfo.UnitIdentifier.Value);
        Assert.Equal("ORGANISASJONSNUMMER", content.ContactPoint.UnitContactInfo.UnitIdentifier.Type);
    }
}
