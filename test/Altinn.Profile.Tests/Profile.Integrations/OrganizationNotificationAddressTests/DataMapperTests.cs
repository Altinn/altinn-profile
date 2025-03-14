using System;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations;

public class DataMapperTests
{
    [Fact]
    public void MapOrganizationNotificationAddress_WhenMappingPhone_Returns()
    {
        // Arrange
        var entry = new Entry() { Id = "37ab4733648c4d5b825a813c6e1ace70", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"mobiltelefon\":{\"navn\":\"4798765432\",\"internasjonaltPrefiks\":\"47\",\"nasjonaltNummer\":\"98765432\"}},\"identifikator\":\"37ab4733648c4d5b825a813c6e1ace70\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920254321\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new Organization { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };
        
        // Act
        var organizationNotificationAddress = DataMapper.MapOrganizationNotificationAddress(entry, organization);

        // Assert
        Assert.Equal("98765432", organizationNotificationAddress.Address);
        Assert.Equal("+47", organizationNotificationAddress.Domain);
        Assert.Equal("+4798765432", organizationNotificationAddress.FullAddress);
        Assert.Equal("37ab4733648c4d5b825a813c6e1ace70", organizationNotificationAddress.RegistryID);
    }

    [Fact]
    public void MapOrganizationNotificationAddress_WhenMappingEmail_Returns()
    {
        // Arrange
        var entry = new Entry() { Id = "27ae0c8bea1f4f02a974c10429c32758", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"epostadresse\":{\"navn\":\"test@test.no\",\"domenenavn\":\"test.no\",\"brukernavn\":\"test\"}},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new Organization { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };

        // Act
        var organizationNotificationAddress = DataMapper.MapOrganizationNotificationAddress(entry, organization);

        // Assert
        Assert.Equal("test", organizationNotificationAddress.Address);
        Assert.Equal("test.no", organizationNotificationAddress.Domain);
        Assert.Equal("test@test.no", organizationNotificationAddress.FullAddress);
        Assert.Equal("27ae0c8bea1f4f02a974c10429c32758", organizationNotificationAddress.RegistryID);
    }
}
