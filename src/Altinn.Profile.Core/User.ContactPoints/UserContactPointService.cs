using Altinn.Platform.Profile.Models;

namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// An implementation of <see cref="IUserContactPointsService"/> that uses the <see cref="IUserProfileService"/> to obtain contact point information.
/// </summary>
public class UserContactPointService : IUserContactPointsService
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
    public async Task<Result<UserContactPointAvailabilityList, bool>> GetContactPointAvailability(List<string> nationalIdentityNumbers)
    {
        UserContactPointAvailabilityList availabilityResult = new();

        foreach (var nationalIdentityNumber in nationalIdentityNumbers)
        {
            Result<UserProfile, bool> result = await _userProfileService.GetUser(nationalIdentityNumber);

            result.Match(
                profile =>
                {
                    availabilityResult.AvailabilityList.Add(new UserContactPointAvailability()
                    {
                        UserId = profile.PartyId,
                        NationalIdentityNumber = profile.Party.SSN,
                        EmailRegistered = !string.IsNullOrEmpty(profile.Email),
                        MobileNumberRegistered = !string.IsNullOrEmpty(profile.PhoneNumber),
                        IsReserved = profile.IsReserved
                    });
                },
                _ => { });
        }

        return availabilityResult;
    }

    /// <inheritdoc/>
    public async Task<Result<UserContactPointsList, bool>> GetContactPoints(List<string> nationalIdentityNumbers)
    {
        UserContactPointsList resultList = new();

        foreach (var nationalIdentityNumber in nationalIdentityNumbers)
        {
            Result<UserProfile, bool> result = await _userProfileService.GetUser(nationalIdentityNumber);

            result.Match(
              profile =>
              {
                  resultList.ContactPointsList.Add(
                    new UserContactPoints()
                    {
                        UserId = profile.PartyId,
                        NationalIdentityNumber = profile.Party.SSN,
                        Email = profile.Email,
                        MobileNumber = profile.PhoneNumber,
                        IsReserved = profile.IsReserved
                    });
              },
              _ => { });
        }

        return resultList;
    }
}
