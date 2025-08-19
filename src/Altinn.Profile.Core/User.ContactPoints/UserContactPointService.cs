using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Models;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// An implementation of <see cref="IUserContactPointsService"/> that uses the <see cref="IUserProfileService"/> to obtain contact point information.
/// </summary>
public class UserContactPointService : IUserContactPointsService
{
    private readonly IUserProfileService _userProfileService;
    private readonly IPersonService _personService;
    private readonly bool _enablePersonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
    /// </summary>
    public UserContactPointService(IUserProfileService userProfileService, IPersonService personService, IOptions<CoreSettings> settings)
    {
        _userProfileService = userProfileService;
        _personService = personService;
        _enablePersonService = settings.Value.EnableLocalKrrFetch;
    }

    /// <inheritdoc/>
    public async Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers)
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
    public async Task<UserContactPointsList> GetContactPoints(List<string> nationalIdentityNumbers)
    {
        UserContactPointsList resultList = new();

        if (_enablePersonService)
        {
            var preferencesForContacts = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers);

            preferencesForContacts.ForEach(contactPreference =>
            {
                resultList.ContactPointsList.Add(
                    new UserContactPoints()
                    {
                        NationalIdentityNumber = contactPreference.NationalIdentityNumber,
                        Email = contactPreference.Email,
                        MobileNumber = contactPreference.MobileNumber,
                        IsReserved = contactPreference.IsReserved,
                    });
            });
        }
        else
        {
            foreach (var nationalIdentityNumber in nationalIdentityNumbers)
            {
                Result<UserProfile, bool> result = await _userProfileService.GetUser(nationalIdentityNumber);

                result.Match(
                    profile =>
                    {
                        resultList.ContactPointsList.Add(
                            new UserContactPoints()
                            {
                                NationalIdentityNumber = profile.Party.SSN,
                                Email = profile.Email,
                                MobileNumber = profile.PhoneNumber,
                                IsReserved = profile.IsReserved,
                            });
                    },
                    _ => { });
            }
        }

        return resultList;
    }
}
