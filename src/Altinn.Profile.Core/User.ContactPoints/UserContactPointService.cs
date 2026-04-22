using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Models;

using static Altinn.Profile.Core.Unit.ContactPoints.CustomContactPointUrn;

namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// An implementation of <see cref="IUserContactPointsService"/> that uses the <see cref="IUserProfileService"/> to obtain contact point information.
/// </summary>
public class UserContactPointService : IUserContactPointsService
{
    private readonly IUserProfileService _userProfileService;
    private readonly IPersonService _personService;
    private readonly IUserContactInfoRepository _userContactInfoRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
    /// </summary>
    public UserContactPointService(IUserProfileService userProfileService, IPersonService personService, IUserContactInfoRepository userContactInfoRepository)
    {
        _userProfileService = userProfileService;
        _personService = personService;
        _userContactInfoRepository = userContactInfoRepository;
    }

    /// <inheritdoc/>
    public async Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers, CancellationToken cancellationToken)
    {
        UserContactPointAvailabilityList availabilityResult = new();

        foreach (var nationalIdentityNumber in nationalIdentityNumbers)
        {
            Result<UserProfile, bool> result = await _userProfileService.GetUser(nationalIdentityNumber, cancellationToken);

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
    public async Task<SelfIdentifiedUserContactPointsList> GetSiContactPoints(List<string> externalIdentities, CancellationToken cancellationToken)
    {
        SelfIdentifiedUserContactPointsList contactPointsList = new();

        foreach (var urnIdentifier in externalIdentities)
        {
            if (!TryParse(urnIdentifier, out CustomContactPointUrn? parsedUrn))
            {
                continue;
            }

            var contactPoint = parsedUrn switch
            {
                IDPortenEmail idportenEmail => await ProcessIdPortenEmail(idportenEmail, urnIdentifier, cancellationToken),
                Username username => await ProcessUsername(username, urnIdentifier, cancellationToken),
                _ => null
            };

            if (contactPoint is not null)
            {
                contactPointsList.ContactPointsList.Add(contactPoint);
            }
        }

        return contactPointsList;
    }

    private async Task<SiUserContactPoints?> ProcessIdPortenEmail(IDPortenEmail idportenEmail, string urnIdentifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idportenEmail.Value.Value))
        {
            return null;
        }

        UserContactInfo? contactInfo;
        try
        {
             contactInfo = await _userContactInfoRepository.GetByUsername("epost:" + idportenEmail.Value.Value, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // If multiple records exist with the same username, SingleOrDefaultAsync will throw an InvalidOperationException. 
            // This should not happen as username is expected to be unique, but if it does, we return null to indicate that a valid user contact info could not be retrieved.
            return null;
        }

        if (contactInfo != null)
        {
            return new SiUserContactPoints()
            {
                Email = contactInfo.EmailAddress ?? idportenEmail.Value.Value,
                MobileNumber = contactInfo.PhoneNumber,
                ExternalIdentity = urnIdentifier
            };
        }
        else
        {
            return new SiUserContactPoints()
            {
                Email = idportenEmail.Value.Value,
                MobileNumber = null,
                ExternalIdentity = urnIdentifier
            };
        }
    }

    private async Task<SiUserContactPoints?> ProcessUsername(Username username, string urnIdentifier, CancellationToken cancellationToken)
    {
        UserContactInfo? contactInfo;
        try
        {
            contactInfo = await _userContactInfoRepository.GetByUsername(username.Value.Value, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // If multiple records exist with the same username, SingleOrDefaultAsync will throw an InvalidOperationException. 
            // This should not happen as username is expected to be unique, but if it does, we return null to indicate that a valid user contact info could not be retrieved.
            return null;
        }

        if (contactInfo != null && (!string.IsNullOrWhiteSpace(contactInfo.EmailAddress) || !string.IsNullOrWhiteSpace(contactInfo.PhoneNumber)))
        {
            return new SiUserContactPoints()
            {
                Email = contactInfo.EmailAddress,
                ExternalIdentity = urnIdentifier,
                MobileNumber = contactInfo.PhoneNumber,
            };
        }
        
        return null;
    }
}
