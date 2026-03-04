using Altinn.Profile.Models;
using Altinn.Register.Contracts;

namespace Altinn.Profile.Core.User
{
    /// <summary>
    /// Provides mapping functionality from <see cref="Party"/> objects to <see cref="UserProfile"/> objects.
    /// </summary>
    internal class UserProfileMapper
    {
        /// <summary>
        /// Maps a <see cref="Party"/> instance to a <see cref="UserProfile"/> instance.
        /// </summary>
        /// <param name="party">The party to map from.</param>
        /// <returns>
        /// A <see cref="UserProfile"/> instance if mapping is successful; otherwise, <c>null</c>.
        /// </returns>
        internal static UserProfile? MapFromParty(Party party)
        {
            if (party == null)
            {
                return null;
            }
             
            if (party is Register.Contracts.Person person)
            {
                var user = person.User.Value;
                return new UserProfile
                {
                    UserId = (int?)user?.UserId.Value ?? 0,
                    UserUuid = person.Uuid,
                    UserName = user?.Username.Value,
                    PartyId = (int)person.PartyId.Value,
                    Party = new Register.Contracts.V1.Party
                    {
                        PartyId = (int)person.PartyId.Value,
                        PartyUuid = person.Uuid,
                        PartyTypeName = Register.Contracts.V1.PartyType.Person,
                        SSN = person.PersonIdentifier.ToString(),
                        Name = person.ShortName.Value,
                        IsDeleted = person.IsDeleted.Value,
                        Person = new Register.Contracts.V1.Person
                        {
                            SSN = person.PersonIdentifier.ToString(),
                            Name = person.ShortName.Value,
                            FirstName = person.FirstName.Value,
                            LastName = person.LastName.Value,
                            MiddleName = person.MiddleName.Value,
                            TelephoneNumber = null,
                            MailingAddress = person.MailingAddress.Value?.Address,
                            MailingPostalCode = person.MailingAddress.Value?.PostalCode,
                            MailingPostalCity = person.MailingAddress.Value?.City,
                            AddressMunicipalNumber = person.Address.Value?.MunicipalNumber,
                            AddressMunicipalName = person.Address.Value?.MunicipalName,
                            AddressStreetName = person.Address.Value?.StreetName,
                            AddressHouseNumber = person.Address.Value?.HouseNumber,
                            AddressHouseLetter = person.Address.Value?.HouseLetter,
                            AddressPostalCode = person.Address.Value?.PostalCode,
                            AddressCity = person.Address.Value?.City,
                            DateOfDeath = person.DateOfDeath.Value.ToDateTime(default),
                        },
                        LastChangedInAltinn = person.ModifiedAt.Value,
                    },
                    UserType = Models.Enums.UserType.SSNIdentified,
                };
            }

            if (party is SelfIdentifiedUser si)
            {
                var user = si.User.Value;
                return new UserProfile
                {
                    UserId = (int?)user?.UserId.Value ?? 0,
                    UserUuid = si.Uuid,
                    UserName = user?.Username.Value,
                    PartyId = (int)si.PartyId.Value,
                    ExternalIdentity = si.ExternalUrn.Value?.ToString(),
                    Party = new Register.Contracts.V1.Party
                    {
                        PartyId = (int)si.PartyId.Value,
                        PartyUuid = si.Uuid,
                        PartyTypeName = Register.Contracts.V1.PartyType.SelfIdentified,
                        SSN = string.Empty,
                        OrgNumber = string.Empty,
                        Name = si.DisplayName.Value,
                        IsDeleted = si.IsDeleted.Value,                      
                        LastChangedInAltinn = si.ModifiedAt.Value,
                    },
                    UserType = Models.Enums.UserType.SelfIdentified,
                };
            }

            return null;
        }
    }
}
