using System.Threading.Tasks;

using Altinn.Profile.Core.User;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;
using Altinn.Register.Contracts;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.User
{
    public class UserProfileMapperTests
    {
        [Fact]
        public async Task MapFromPerson()
        {
            var input = await TestDataLoader.Load<Person>("person-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Person
            var expected = await TestDataLoader.Load<UserProfile>("person-expected");

            var result = UserProfileMapper.MapFromPerson(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.SSN, result.Party.SSN);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);

            // Nested Person fields
            Assert.Equal(expected.Party.Person.FirstName, result.Party.Person.FirstName);
            Assert.Equal(expected.Party.Person.LastName, result.Party.Person.LastName);
            Assert.Equal(expected.Party.Person.MiddleName, result.Party.Person.MiddleName);
            Assert.Equal(expected.Party.Person.TelephoneNumber, result.Party.Person.TelephoneNumber);
            Assert.Equal(expected.Party.Person.MailingAddress, result.Party.Person.MailingAddress);
            Assert.Equal(expected.Party.Person.MailingPostalCode, result.Party.Person.MailingPostalCode);
            Assert.Equal(expected.Party.Person.MailingPostalCity, result.Party.Person.MailingPostalCity);
            Assert.Equal(expected.Party.Person.AddressMunicipalNumber, result.Party.Person.AddressMunicipalNumber);
            Assert.Equal(expected.Party.Person.AddressMunicipalName, result.Party.Person.AddressMunicipalName);
            Assert.Equal(expected.Party.Person.AddressStreetName, result.Party.Person.AddressStreetName);
            Assert.Equal(expected.Party.Person.AddressHouseNumber, result.Party.Person.AddressHouseNumber);
            Assert.Equal(expected.Party.Person.AddressHouseLetter, result.Party.Person.AddressHouseLetter);
            Assert.Equal(expected.Party.Person.AddressPostalCode, result.Party.Person.AddressPostalCode);
            Assert.Equal(expected.Party.Person.AddressCity, result.Party.Person.AddressCity);
            Assert.Equal(expected.Party.Person.DateOfDeath, result.Party.Person.DateOfDeath);
        }

        [Fact(Skip = "Not yet ready for testing as ExternalIdentity is not correct")]
        public async Task MapFromSiUser_EduUser_InputToExpected()
        {
            var input = await TestDataLoader.Load<SelfIdentifiedUser>("siuser-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Register
            var expected = await TestDataLoader.Load<UserProfile>("siuser-expected");

            var result = UserProfileMapper.MapFromSiUser(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.ExternalIdentity, result.ExternalIdentity); // SI-specific
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);
            Assert.Empty(result.Party.SSN);      // always empty string in mapper
            Assert.Empty(result.Party.OrgNumber); // always empty string in mapper
            Assert.Null(result.Party.Person);    // no Person for SI users
        }

        [Fact(Skip = "Not yet ready for testing as ExternalIdentity is not correct")]
        public async Task MapFromSiUser_LegacyUser_InputToExpected()
        {
            var input = await TestDataLoader.Load<SelfIdentifiedUser>("legacy-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Register
            var expected = await TestDataLoader.Load<UserProfile>("legacy-expected");

            var result = UserProfileMapper.MapFromSiUser(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.ExternalIdentity, result.ExternalIdentity); // SI-specific
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);
            Assert.Empty(result.Party.SSN);      // always empty string in mapper
            Assert.Empty(result.Party.OrgNumber); // always empty string in mapper
            Assert.Null(result.Party.Person);    // no Person for SI users
        }

        [Fact]
        public async Task MapFromSiUser_EmailyUser_InputToExpected()
        {
            var input = await TestDataLoader.Load<SelfIdentifiedUser>("emailuser-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Register
            var expected = await TestDataLoader.Load<UserProfile>("emailuser-expected");

            var result = UserProfileMapper.MapFromSiUser(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.ExternalIdentity, result.ExternalIdentity); // SI-specific
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);
            Assert.Empty(result.Party.SSN);      // always empty string in mapper
            Assert.Empty(result.Party.OrgNumber); // always empty string in mapper
            Assert.Null(result.Party.Person);    // no Person for SI users
        }

        [Fact]
        public async Task MapFromParty_EmailyUser_InputToExpected()
        {
            var input = await TestDataLoader.Load<SelfIdentifiedUser>("emailuser-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Register
            var expected = await TestDataLoader.Load<UserProfile>("emailuser-expected");

            var result = UserProfileMapper.MapFromParty(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.ExternalIdentity, result.ExternalIdentity); // SI-specific
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);
            Assert.Empty(result.Party.SSN);      // always empty string in mapper
            Assert.Empty(result.Party.OrgNumber); // always empty string in mapper
            Assert.Null(result.Party.Person);    // no Person for SI users
        }

        [Fact]
        public async Task MapFromParty_Person_Input_ToExpected()
        {
            var input = await TestDataLoader.Load<Person>("person-input");

            // Note that expected is userProfile without portal settings preferences, as those are not mapped from Person
            var expected = await TestDataLoader.Load<UserProfile>("person-expected");

            var result = UserProfileMapper.MapFromParty(input);

            // Top-level UserProfile fields
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.UserUuid, result.UserUuid);
            Assert.Equal(expected.UserName, result.UserName);
            Assert.Equal(expected.PartyId, result.PartyId);
            Assert.Equal(expected.UserType, result.UserType);

            // Party fields
            Assert.Equal(expected.Party.PartyId, result.Party.PartyId);
            Assert.Equal(expected.Party.PartyUuid, result.Party.PartyUuid);
            Assert.Equal(expected.Party.PartyTypeName, result.Party.PartyTypeName);
            Assert.Equal(expected.Party.SSN, result.Party.SSN);
            Assert.Equal(expected.Party.Name, result.Party.Name);
            Assert.Equal(expected.Party.IsDeleted, result.Party.IsDeleted);
            Assert.Equal(expected.Party.LastChangedInAltinn, result.Party.LastChangedInAltinn);

            // Nested Person fields
            Assert.Equal(expected.Party.Person.FirstName, result.Party.Person.FirstName);
            Assert.Equal(expected.Party.Person.LastName, result.Party.Person.LastName);
            Assert.Equal(expected.Party.Person.MiddleName, result.Party.Person.MiddleName);
            Assert.Equal(expected.Party.Person.TelephoneNumber, result.Party.Person.TelephoneNumber);
            Assert.Equal(expected.Party.Person.MailingAddress, result.Party.Person.MailingAddress);
            Assert.Equal(expected.Party.Person.MailingPostalCode, result.Party.Person.MailingPostalCode);
            Assert.Equal(expected.Party.Person.MailingPostalCity, result.Party.Person.MailingPostalCity);
            Assert.Equal(expected.Party.Person.AddressMunicipalNumber, result.Party.Person.AddressMunicipalNumber);
            Assert.Equal(expected.Party.Person.AddressMunicipalName, result.Party.Person.AddressMunicipalName);
            Assert.Equal(expected.Party.Person.AddressStreetName, result.Party.Person.AddressStreetName);
            Assert.Equal(expected.Party.Person.AddressHouseNumber, result.Party.Person.AddressHouseNumber);
            Assert.Equal(expected.Party.Person.AddressHouseLetter, result.Party.Person.AddressHouseLetter);
            Assert.Equal(expected.Party.Person.AddressPostalCode, result.Party.Person.AddressPostalCode);
            Assert.Equal(expected.Party.Person.AddressCity, result.Party.Person.AddressCity);
            Assert.Equal(expected.Party.Person.DateOfDeath, result.Party.Person.DateOfDeath);
        }

        [Fact]
        public async Task MapFromParty_WhenNull_ReturnsNull()
        {
            var result = UserProfileMapper.MapFromParty(null);
            Assert.Null(result);
        }
    }
}
