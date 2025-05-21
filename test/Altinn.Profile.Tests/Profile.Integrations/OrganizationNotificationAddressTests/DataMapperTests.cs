using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Xunit;
using static Altinn.Profile.Models.OrgNotificationAddressesResponse;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests;

public class DataMapperTests
{
    [Theory]
    [InlineData("47")]
    [InlineData("+47")]
    [InlineData("0047")]
    public void PopulateOrganizationNotificationAddress_WhenMappingPhone_Returns(string prefix)
    {
        // Arrange
        var entry = new Entry() { Id = "37ab4733648c4d5b825a813c6e1ace70", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"mobiltelefon\":{\"navn\":\"4798765432\",\"internasjonaltPrefiks\":\"" + prefix + "\",\"nasjonaltNummer\":\"98765432\"}},\"identifikator\":\"37ab4733648c4d5b825a813c6e1ace70\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920254321\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };

        // Act
        var organizationNotificationAddress = DataMapper.PopulateOrganizationNotificationAddress(organization, entry);

        // Assert
        Assert.Equal("98765432", organizationNotificationAddress.Address);
        Assert.Equal("+47", organizationNotificationAddress.Domain);
        Assert.Equal("+4798765432", organizationNotificationAddress.FullAddress);
        Assert.Equal("37ab4733648c4d5b825a813c6e1ace70", organizationNotificationAddress.RegistryID);
    }

    [Fact]
    public void PopulateOrganizationNotificationAddress_WhenMappingPhoneWithoutPrefix_Returns()
    {
        // Arrange
        var entry = new Entry() { Id = "37ab4733648c4d5b825a813c6e1ace70", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"mobiltelefon\":{\"navn\":\"4798765432\",\"internasjonaltPrefiks\":\"\",\"nasjonaltNummer\":\"98765432\"}},\"identifikator\":\"37ab4733648c4d5b825a813c6e1ace70\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920254321\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };

        // Act
        var organizationNotificationAddress = DataMapper.PopulateOrganizationNotificationAddress(organization, entry);

        // Assert
        Assert.Equal("98765432", organizationNotificationAddress.Address);
        Assert.Null(organizationNotificationAddress.Domain);
        Assert.Equal("98765432", organizationNotificationAddress.FullAddress);
        Assert.Equal("37ab4733648c4d5b825a813c6e1ace70", organizationNotificationAddress.RegistryID);
    }

    [Fact]
    public void PopulateOrganizationNotificationAddress_WhenMappingEmail_Returns()
    {
        // Arrange
        var entry = new Entry() { Id = "27ae0c8bea1f4f02a974c10429c32758", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"epostadresse\":{\"navn\":\"test@test.no\",\"domenenavn\":\"test.no\",\"brukernavn\":\"test\"}},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };

        // Act
        var organizationNotificationAddress = DataMapper.PopulateOrganizationNotificationAddress(organization, entry);

        // Assert
        Assert.Equal("test", organizationNotificationAddress.Address);
        Assert.Equal("test.no", organizationNotificationAddress.Domain);
        Assert.Equal("test@test.no", organizationNotificationAddress.FullAddress);
        Assert.Equal("27ae0c8bea1f4f02a974c10429c32758", organizationNotificationAddress.RegistryID);
    }

    [Fact]
    public void PopulateOrganizationNotificationAddress_WhenMappingNoType_Throws()
    {
        // Arrange
        var entry = new Entry() { Id = "27ae0c8bea1f4f02a974c10429c32758", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var organization = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };

        // Act & Assert
        Assert.Throws<OrganizationNotificationAddressChangesException>(() => DataMapper.PopulateOrganizationNotificationAddress(organization, entry));
    }

    [Fact]
    public void PopulateExistingOrganizationNotificationAddress_WhenMappingEmail_SetsCorrectValues()
    {
        // Arrange
        var entry = new Entry() { Id = "27ae0c8bea1f4f02a974c10429c32758", ContentStringified = "{\"Kontaktinformasjon\":{\"digitalVarslingsinformasjon\":{\"epostadresse\":{\"navn\":\"test@test.no\",\"domenenavn\":\"test.no\",\"brukernavn\":\"test\"}},\"identifikator\":\"27ae0c8bea1f4f02a974c10429c32758\",\"kontaktinformasjonForEnhet\":{\"enhetsidentifikator\":{\"verdi\":\"920212345\",\"type\":\"ORGANISASJONSNUMMER\"}}}}" };
        var address = new NotificationAddressDE { Address = "old", Domain = "address.com", AddressType = AddressType.Email, HasRegistryAccepted = false, RegistryOrganizationId = 1, UpdateSource = UpdateSource.Altinn, RegistryID = "27ae0c8bea1f4f02a974c10429c32758" };

        // Act
        var organizationNotificationAddress = DataMapper.PopulateExistingOrganizationNotificationAddress(address, entry);

        // Assert
        Assert.Equal("test", organizationNotificationAddress.Address);
        Assert.Equal("test.no", organizationNotificationAddress.Domain);
        Assert.Equal("test@test.no", organizationNotificationAddress.FullAddress);
        Assert.Equal("27ae0c8bea1f4f02a974c10429c32758", organizationNotificationAddress.RegistryID);
        Assert.Equal(UpdateSource.KoFuVi, organizationNotificationAddress.UpdateSource);
        Assert.True(organizationNotificationAddress.HasRegistryAccepted);
    }

    [Fact]
    public void MapToRegistryRequest_WhenEmailAddress_MapsToEmail()
    {
        // Arrange
        var organization = new Organization { OrganizationNumber = "123456789" };
        var notificationAddress = new NotificationAddress { AddressType = AddressType.Email, Address = "test", Domain = "test.com" };

        // Act & Assert
        var request = DataMapper.MapToRegistryRequest(notificationAddress, organization.OrganizationNumber);

        Assert.Equal("123456789", request.ContactInfo.UnitContactInfo.UnitIdentifier.Value);
        Assert.Equal("ORGANISASJONSNUMMER", request.ContactInfo.UnitContactInfo.UnitIdentifier.Type);
        Assert.Equal("test", request.ContactInfo.DigitalContactPoint.EmailAddress.Username);
        Assert.Equal("test.com", request.ContactInfo.DigitalContactPoint.EmailAddress.Domain);
    }

    [Fact]
    public void MapToRegistryRequest_WhenPhoneNumber_MapsToEmail()
    {
        // Arrange
        var organization = new Organization { OrganizationNumber = "123456789" };
        var notificationAddress = new NotificationAddress { AddressType = AddressType.SMS, Address = "98765432", Domain = "+47" };

        // Act & Assert
        var request = DataMapper.MapToRegistryRequest(notificationAddress, organization.OrganizationNumber);

        Assert.Equal("123456789", request.ContactInfo.UnitContactInfo.UnitIdentifier.Value);
        Assert.Equal("ORGANISASJONSNUMMER", request.ContactInfo.UnitContactInfo.UnitIdentifier.Type);
        Assert.Equal("98765432", request.ContactInfo.DigitalContactPoint.PhoneNumber.NationalNumber);
        Assert.Equal("+47", request.ContactInfo.DigitalContactPoint.PhoneNumber.Prefix);
    }

    [Fact]
    public void MapFromCoreModelNotificationAddress_WhenEmailAddress_MapsCorrectly()
    {
        // Arrange
        var organizationDE = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };
        var notificationAddress = new NotificationAddress { AddressType = AddressType.Email, Address = "test", Domain = "test.com" };

        // Act & Assert
        var notificationAddressDE = DataMapper.MapFromCoreModelForNewNotificationAddress(organizationDE, notificationAddress, "1");

        Assert.Equal(notificationAddress.AddressType, notificationAddressDE.AddressType);
        Assert.Equal(notificationAddress.Address, notificationAddressDE.Address);
        Assert.Equal(notificationAddress.Domain, notificationAddressDE.Domain);
        Assert.Equal(notificationAddress.NotificationName, notificationAddressDE.NotificationName);
        Assert.Equal("1", notificationAddressDE.RegistryID);
        Assert.Equal(UpdateSource.Altinn, notificationAddressDE.UpdateSource);
        Assert.False(notificationAddressDE.IsSoftDeleted);
    }

    [Fact]
    public void MapFromCoreModelNotificationAddress_WhenPhoneNumber_MapsCorrectly()
    {
        // Arrange
        var organizationDE = new OrganizationDE { RegistryOrganizationNumber = "123456789", RegistryOrganizationId = 1 };
        var notificationAddress = new NotificationAddress { AddressType = AddressType.SMS, Address = "98765432", Domain = "+47" };

        // Act & Assert
        var notificationAddressDE = DataMapper.MapFromCoreModelForNewNotificationAddress(organizationDE, notificationAddress, "id");

        Assert.Equal(notificationAddress.AddressType, notificationAddressDE.AddressType);
        Assert.Equal(notificationAddress.Address, notificationAddressDE.Address);
        Assert.Equal(notificationAddress.Domain, notificationAddressDE.Domain);
        Assert.Equal(notificationAddress.NotificationName, notificationAddressDE.NotificationName);
        Assert.Equal("id", notificationAddressDE.RegistryID);
        Assert.Equal(UpdateSource.Altinn, notificationAddressDE.UpdateSource);
        Assert.False(notificationAddressDE.IsSoftDeleted);
    }
}
