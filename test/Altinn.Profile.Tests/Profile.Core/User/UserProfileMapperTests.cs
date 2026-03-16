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

        [Fact]
        public async Task MapFromSiUser_InputToExpected()
        {
            var input = await TestDataLoader.Load<Register.Contracts.SelfIdentifiedUser>("siuser-input");
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
    }
}
