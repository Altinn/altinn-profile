using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Models;
using Altinn.Profile.Services.Interfaces;

namespace Altinn.Profile.Services.Implementation
{
    /// <summary>
    /// An implementation of <cref="IUserContactPoints"/> that uses the <cref="IUserProfiles"/> to obtain contact point information.
    /// </summary>
    public class UserContactPointService : IUserContactPoints
    {
        private readonly IUserProfiles _userProfiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
        /// </summary>
        public UserContactPointService(IUserProfiles userProfiles)
        {
            _userProfiles = userProfiles;
        }

        /// <inheritdoc/>
        public async Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers)
        {
            UserContactPointAvailabilityList result = new();

            foreach (var nationalIdentityNumber in nationalIdentityNumbers)
            {
                UserProfile profile = await _userProfiles.GetUser(nationalIdentityNumber);

                if (profile == null)
                {
                    continue;
                }

                result.List.Add(new UserContactPointAvailability()
                {
                    UserId = profile.PartyId,
                    NationalIdentityNumber = profile.Party.SSN,
                    EmailRegistered = !string.IsNullOrEmpty(profile.Email),
                    MobileNumberRegistered = !string.IsNullOrEmpty(profile.PhoneNumber),
                    IsReserved = profile.IsReserved
                });
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<UserContactPointsList> GetContactPoints(List<string> nationalIdentityNumbers)
        {
            UserContactPointsList result = new();

            foreach (var nationalIdentityNumber in nationalIdentityNumbers)
            {
                var profile = await _userProfiles.GetUser(nationalIdentityNumber);

                if (profile == null)
                {
                    continue;
                }

                result.List.Add(
                new UserContactPoints()
                {
                    UserId = profile.PartyId,
                    NationalIdentityNumber = profile.Party.SSN,
                    Email = profile.Email,
                    MobileNumber = profile.PhoneNumber,
                    IsReserved = profile.IsReserved
                });
            }

            return result;
        }
    }
}
