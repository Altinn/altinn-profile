using Altinn.Profile.Models;
using Altinn.Register.Contracts;

namespace Altinn.Profile.Core.User
{
    /// <summary>
    /// Provides mapping functionality from <see cref="Party"/> objects to <see cref="UserProfile"/> objects.
    /// </summary>
    public static class UserProfileMapper
    {
        /// <summary>
        /// Maps a <see cref="Party"/> instance to a <see cref="UserProfile"/> instance.
        /// </summary>
        /// <param name="party">The party to map from.</param>
        /// <returns>
        /// A <see cref="UserProfile"/> instance if mapping is successful; otherwise, <c>null</c>.
        /// </returns>
        public static UserProfile? MapFromParty(Party? party)
        {
            if (party == null)
            {
                return null;
            }

            if (party is Register.Contracts.Person person)
            {
                return MapFromPerson(person);
            }

            if (party is SelfIdentifiedUser si)
            {
                return MapFromSiUser(si);
            }

            return null;
        }

        /// <summary>
        /// Provides mapping functionality from <see cref="Register.Contracts.Person"/> objects to <see cref="UserProfile"/> objects.
        /// </summary>
        /// <param name="person">The person object to map from.</param>
        /// <returns> A <see cref="UserProfile"/> instance.</returns>
        public static UserProfile MapFromPerson(Register.Contracts.Person person)
        {
            var user = person.User.Value;
            return new UserProfile
            {
                UserId = (int?)user?.UserId.Value ?? 0,
                UserUuid = person.Uuid,
                UserName = user?.Username.Value ?? string.Empty,
                ExternalIdentity = string.Empty,
                PartyId = (int)person.PartyId.Value,
                Party = new Register.Contracts.V1.Party
                {
                    PartyId = (int)person.PartyId.Value,
                    PartyUuid = person.Uuid,
                    PartyTypeName = Register.Contracts.V1.PartyType.Person,
                    OrgNumber = string.Empty,
                    SSN = person.PersonIdentifier.ToString(),
                    Name = person.ShortName.Value,
                    IsDeleted = person.IsDeleted.Value,
                    Person = new Register.Contracts.V1.Person
                    {
                        SSN = person.PersonIdentifier.ToString(),
                        Name = person.ShortName.Value,
                        FirstName = person.FirstName.Value,
                        LastName = person.LastName.Value,
                        MiddleName = person.MiddleName.Value ?? string.Empty,
                        TelephoneNumber = string.Empty,
                        MobileNumber = string.Empty,
                        MailingAddress = person.MailingAddress.Value?.Address ?? string.Empty,
                        MailingPostalCode = person.MailingAddress.Value?.PostalCode ?? string.Empty,
                        MailingPostalCity = person.MailingAddress.Value?.City ?? string.Empty,
                        AddressMunicipalNumber = person.Address.Value?.MunicipalNumber ?? string.Empty,
                        AddressMunicipalName = person.Address.Value?.MunicipalName ?? string.Empty,
                        AddressStreetName = person.Address.Value?.StreetName ?? string.Empty,
                        AddressHouseNumber = person.Address.Value?.HouseNumber ?? string.Empty,
                        AddressHouseLetter = person.Address.Value?.HouseLetter ?? string.Empty,
                        AddressPostalCode = person.Address.Value?.PostalCode ?? string.Empty,
                        AddressCity = person.Address.Value?.City ?? string.Empty,
                        DateOfDeath = person.DateOfDeath.HasValue ? person.DateOfDeath.Value.ToDateTime(default) : null,
                    },
                    LastChangedInAltinn = person.ModifiedAt.Value,
                },
                UserType = Models.Enums.UserType.SSNIdentified,
            };
        }

        /// <summary>
        /// Provides mapping functionality from <see cref="Register.Contracts.SelfIdentifiedUser"/> objects to <see cref="UserProfile"/> objects.
        /// </summary>
        /// <param name="si">The siuser object to map from.</param>
        /// <returns> A <see cref="UserProfile"/> instance.</returns>
        public static UserProfile MapFromSiUser(SelfIdentifiedUser si)
        {
            var user = si.User.Value;
            var selfIdentifiedUserType = si.SelfIdentifiedUserType.Value;
            var displayName = si.DisplayName.Value;

            if (selfIdentifiedUserType == SelfIdentifiedUserType.IdPortenEmail)
            {
                displayName = "epost:" + displayName;
            }

            return new UserProfile
            {
                UserId = (int?)user?.UserId.Value ?? 0,
                UserUuid = si.Uuid,
                UserName = user?.Username.Value,
                PartyId = (int)si.PartyId.Value,
                ExternalIdentity = si.ExternalUrn.Value?.ToString(),
                PhoneNumber = string.Empty,
                Party = new Register.Contracts.V1.Party
                {
                    PartyId = (int)si.PartyId.Value,
                    PartyUuid = si.Uuid,
                    PartyTypeName = Register.Contracts.V1.PartyType.SelfIdentified,
                    SSN = string.Empty,
                    OrgNumber = string.Empty,
                    Name = displayName,
                    IsDeleted = si.IsDeleted.Value,
                    LastChangedInAltinn = si.ModifiedAt.Value,
                },
                UserType = Models.Enums.UserType.SelfIdentified,
            };
        }
    }
}
