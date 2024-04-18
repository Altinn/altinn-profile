using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core.User;

namespace Altinn.Profile.Core.User.ContactPoints
{
    /// <summary>
    /// An implementation of <see cref="IUserContactPoints"/> that uses the <see cref="IUserProfileService"/> to obtain contact point information.
    /// </summary>
    public class UserContactPointService : IUserContactPoints
    {
        private readonly IUserProfileService _userProfileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
        /// </summary>
        public UserContactPointService(IUserProfileService userProfiles)
        {
            _userProfileService = userProfiles;
        }

        /// <inheritdoc/>
        public async Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers)
        {
            UserContactPointAvailabilityList result = new();

            foreach (var nationalIdentityNumber in nationalIdentityNumbers)
            {
                UserProfile profile = await _userProfileService.GetUser(nationalIdentityNumber);

                if (profile == null)
                {
                    continue;
                }

                result.AvailabilityList.Add(new UserContactPointAvailability()
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
                var profile = await _userProfileService.GetUser(nationalIdentityNumber);

                if (profile == null)
                {
                    continue;
                }

                result.ContactPointList.Add(
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
