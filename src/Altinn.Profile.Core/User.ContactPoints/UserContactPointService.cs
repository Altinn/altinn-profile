using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactInfo;

using Microsoft.Extensions.Logging;

using static Altinn.Profile.Core.Unit.ContactPoints.CustomContactPointUrn;

namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// An implementation of <see cref="IUserContactPointsService"/> that uses the <see cref="IPersonService"/> to obtain contact point information.
/// </summary>
public class UserContactPointService : IUserContactPointsService
{
    private const int ActiveContactPointMonths = 18;

    private readonly IPersonService _personService;
    private readonly IUserContactInfoRepository _userContactInfoRepository;
    private readonly ILogger<UserContactPointService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactPointService"/> class.
    /// </summary>
    /// <param name="personService">The person service used to retrieve contact preferences.</param>
    /// <param name="userContactInfoRepository">The repository used to retrieve self-identified user contact information.</param>
    /// <param name="logger">The logger instance.</param>
    public UserContactPointService(IPersonService personService, IUserContactInfoRepository userContactInfoRepository, ILogger<UserContactPointService> logger)
    {
        _personService = personService;
        _userContactInfoRepository = userContactInfoRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<UserContactPointAvailabilityList> GetContactPointAvailability(List<string> nationalIdentityNumbers, CancellationToken cancellationToken)
    {
        UserContactPointAvailabilityList availabilityResult = new();
        var contactPreferences = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers, cancellationToken);

        foreach (var contactPreference in contactPreferences)
        {
            availabilityResult.AvailabilityList.Add(new UserContactPointAvailability()
            {
                NationalIdentityNumber = contactPreference.NationalIdentityNumber,
                EmailRegistered = !string.IsNullOrWhiteSpace(contactPreference.Email),
                MobileNumberRegistered = !string.IsNullOrWhiteSpace(contactPreference.MobileNumber),
                IsReserved = contactPreference.IsReserved
            });
        }

        return availabilityResult;
    }

    /// <inheritdoc/>
    public async Task<UserContactPointsList> GetContactPoints(
        List<string> nationalIdentityNumbers, bool includeOutdatedContactInfo, CancellationToken cancellationToken)
    {
        UserContactPointsList resultList = new();
        DateTime cutoffDate = DateTime.UtcNow.AddMonths(-ActiveContactPointMonths);

        var contactPreferences = await _personService.GetContactPreferencesAsync(nationalIdentityNumbers, cancellationToken);

        foreach (var contactPreference in contactPreferences)
        {
            bool emailIsOutdated = IsContactPointTooOld(contactPreference.EmailLastUpdatedOrVerified, cutoffDate);
            bool mobileIsOutdated = IsContactPointTooOld(contactPreference.MobileNumberLastUpdatedOrVerified, cutoffDate);

            bool bothContactPointsOutdated = emailIsOutdated && mobileIsOutdated;
            if (!includeOutdatedContactInfo && bothContactPointsOutdated)
            {
                continue;
            }

            UserContactPoints contactPoint = new()
            {
                NationalIdentityNumber = contactPreference.NationalIdentityNumber,
                Email = emailIsOutdated && !includeOutdatedContactInfo ? null : contactPreference.Email,
                EmailIsOutdated = emailIsOutdated,
                MobileNumber = mobileIsOutdated && !includeOutdatedContactInfo ? null : contactPreference.MobileNumber,
                MobileNumberIsOutdated = mobileIsOutdated,
                IsReserved = contactPreference.IsReserved
            };

            resultList.ContactPointsList.Add(contactPoint);
        }

        return resultList;
    }

    /// <inheritdoc/>
    public async Task<DashboardUserContactPoint?> GetContactPointsForDashboard(string nationalIdentityNumber, CancellationToken cancellationToken)
    {
        var contactPreferences = await _personService.GetContactPreferencesAsync([nationalIdentityNumber], cancellationToken);
        if (contactPreferences.Count == 0)
        {
            return null;
        }

        if (contactPreferences.Count > 1)
        {
            throw new InvalidOperationException("Indecisive contact points result");
        }

        var contactPreference = contactPreferences[0];

        DashboardUserContactPoint contactPoint = new()
        {
            NationalIdentityNumber = contactPreference.NationalIdentityNumber,
            Email = contactPreference.Email,
            MobileNumber = contactPreference.MobileNumber,
            IsReserved = contactPreference.IsReserved,
            MobileNumberLastUpdatedOrVerified = contactPreference.MobileNumberLastUpdatedOrVerified,
            EmailLastUpdatedOrVerified = contactPreference.EmailLastUpdatedOrVerified
        };

        return contactPoint;
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

    private static bool IsContactPointTooOld(DateTime? lastTouched, DateTime cutoffDate)
    {
        return !lastTouched.HasValue || lastTouched.Value < cutoffDate;
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
        catch (InvalidOperationException ex)
        {
            // If multiple records exist with the same username, SingleOrDefaultAsync will throw an InvalidOperationException. 
            // This should not happen as username is expected to be unique, but if it does, we return null to indicate that a valid user contact info could not be retrieved.
            _logger.LogError(ex, "Multiple records found for IDPorten email {Email}. Unable to determine contact points.", idportenEmail.Value.Value);
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
        catch (InvalidOperationException ex)
        {
            // If multiple records exist with the same username, SingleOrDefaultAsync will throw an InvalidOperationException. 
            // This should not happen as username is expected to be unique, but if it does, we return null to indicate that a valid user contact info could not be retrieved.
            _logger.LogError(ex, "Multiple records found for username {Username}. Unable to determine contact points.", username.Value.Value);
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
