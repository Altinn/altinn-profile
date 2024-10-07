using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Models;

namespace Altinn.Profile.UseCases;

/// <summary>
/// Provides an implementation for retrieving user contact details based on specified lookup criteria.
/// </summary>
public class UserContactDetailsRetriever : IUserContactDetailsRetriever
{
    private readonly IRegisterService _registerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContactDetailsRetriever"/> class.
    /// </summary>
    /// <param name="registerService">The register service for retrieving user contact details.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registerService"/> is null.</exception>
    public UserContactDetailsRetriever(IRegisterService registerService)
    {
        _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
    }

    /// <summary>
    /// Asynchronously retrieves the contact details for one or more users based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookupCriteria">The user contact point lookup criteria, which includes national identity numbers.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> 
    /// where the value is <see cref="UserContactDetailsLookupResult"/> and the error is <see cref="bool"/>.
    /// </returns>
    public async Task<Result<UserContactDetailsLookupResult, bool>> RetrieveAsync(UserContactPointLookup lookupCriteria)
    {
        if (lookupCriteria?.NationalIdentityNumbers == null || lookupCriteria.NationalIdentityNumbers.Count == 0)
        {
            return false;
        }

        var userContactDetails = await _registerService.GetUserContactAsync(lookupCriteria.NationalIdentityNumbers);

        return userContactDetails.Match(
            MapToUserContactDetailsResult,
            _ => false);
    }

    /// <summary>
    /// Maps an <see cref="IUserContact"/> to a <see cref="UserContactDetails"/>.
    /// </summary>
    /// <param name="userContactDetails">The user contact details to map.</param>
    /// <returns>The mapped <see cref="UserContactDetails"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userContactDetails"/> is null.</exception>
    private UserContactDetails MapToUserContactDetails(IUserContact userContactDetails)
    {
        ArgumentNullException.ThrowIfNull(userContactDetails, nameof(userContactDetails));

        return new UserContactDetails
        {
            Reservation = userContactDetails.IsReserved,
            EmailAddress = userContactDetails.EmailAddress,
            LanguageCode = userContactDetails.LanguageCode,
            MobilePhoneNumber = userContactDetails.MobilePhoneNumber,
            NationalIdentityNumber = userContactDetails.NationalIdentityNumber
        };
    }

    /// <summary>
    /// Maps the user contact details lookup result to a <see cref="UserContactDetailsLookupResult"/>.
    /// </summary>
    /// <param name="userContactResult">The user contact details lookup result.</param>
    /// <returns>A <see cref="Result{TValue, TError}"/> containing the mapped user contact details.</returns>
    private Result<UserContactDetailsLookupResult, bool> MapToUserContactDetailsResult(IUserContactResult userContactResult)
    {
        var unmatchedNationalIdentityNumbers = userContactResult?.UnmatchedNationalIdentityNumbers ?? null;
        var matchedUserContactDetails = userContactResult?.MatchedUserContact?.Select(MapToUserContactDetails).ToImmutableList();

        return new UserContactDetailsLookupResult(matchedUserContactDetails, unmatchedNationalIdentityNumbers);
    }
}
