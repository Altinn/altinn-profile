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

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
    /// <summary>
    /// Initializes a new instance of UserContactPointService with the required user profile and person services.
    /// </summary>
    public UserContactPointService(IUserProfileService userProfileService, IPersonService personService)
    {
        _userProfileService = userProfileService;
        _personService = personService;
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
                        NationalIdentityNumber = profile.Party?.SSN ?? string.Empty,
                        EmailRegistered = !string.IsNullOrEmpty(profile.Email),
                        MobileNumberRegistered = !string.IsNullOrEmpty(profile.PhoneNumber),
                        IsReserved = profile.IsReserved
                    });
                },
                _ => { });
        }

        return availabilityResult;
    }

    /// <summary>
    /// Retrieves contact point details for the specified national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">The national identity numbers to retrieve contact points for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A UserContactPointsList containing contact point entries for each retrieved contact preference.</returns>
    public async Task<UserContactPointsList> GetContactPoints(List<string> nationalIdentityNumbers, CancellationToken cancellationToken)
    {
        UserContactPointsList resultList = new();

        var preferencesForContacts = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers, cancellationToken);

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

        return resultList;
    }
}
