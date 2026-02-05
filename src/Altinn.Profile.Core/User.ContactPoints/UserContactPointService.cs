using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// An implementation of <see cref="IUserContactPointsService"/> that uses the <see cref="IUserProfileService"/> to obtain contact point information.
/// </summary>
public class UserContactPointService : IUserContactPointsService
{
    private const string _urnPrefix = "urn:altinn:person:idporten-email::";
    private readonly IUserProfileService _userProfileService;
    private readonly IPersonService _personService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Task<SelfIdentifiedUserContactPointsList> GetSiContactPoints(List<Uri> externalIdentities, CancellationToken cancellationToken)
    {
        SelfIdentifiedUserContactPointsList resultList = new();

        foreach (var emailIdentifier in externalIdentities)
        {
            if (!emailIdentifier.ToString().StartsWith(_urnPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string email = emailIdentifier.ToString()[_urnPrefix.Length..].Trim();
            if (email.Length == 0)
            {
                continue;
            }

            resultList.ContactPointsList.Add(new SiUserContactPoints()
            {
                Email = email,
                MobileNumber = string.Empty
            });
        }

        return Task.FromResult(resultList);
    }
}
