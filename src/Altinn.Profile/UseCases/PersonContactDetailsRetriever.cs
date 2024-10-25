using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Services;
using Altinn.Profile.Models;

namespace Altinn.Profile.UseCases;

/// <summary>
/// Provides an implementation for retrieving the contact details for one or more persons.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PersonContactDetailsRetriever"/> class.
/// </remarks>
/// <param name="personService">The person service for retrieving contact details.</param>
public class PersonContactDetailsRetriever(IPersonService personService) : IPersonContactDetailsRetriever
{
    private readonly IPersonService _personService = personService;

    /// <summary>
    /// Asynchronously retrieves the contact details for one or more persons based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookupCriteria">The criteria used to look up contact details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="PersonContactDetailsLookupResult"/> represents the successful outcome and <see cref="bool"/> indicates a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lookupCriteria"/> is null.</exception>
    public async Task<Result<PersonContactDetailsLookupResult, bool>> RetrieveAsync(UserContactDetailsLookupCriteria lookupCriteria)
    {
        ArgumentNullException.ThrowIfNull(lookupCriteria);

        if (lookupCriteria.NationalIdentityNumbers == null || lookupCriteria.NationalIdentityNumbers.Count == 0)
        {
            return false;
        }

        var contactDetails = await _personService.GetContactPreferencesAsync(lookupCriteria.NationalIdentityNumbers);

        return contactDetails.Match(
            MapToContactDetailsLookupResult,
            _ => false);
    }

    /// <summary>
    /// Maps the person contact details to a <see cref="PersonContactDetails"/>.
    /// </summary>
    /// <param name="contactPreferences">The person contact details to map.</param>
    /// <returns>The mapped <see cref="PersonContactDetails"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contactPreferences"/> is null.</exception>
    private PersonContactDetails MapToContactDetails(PersonContactPreferences contactPreferences)
    {
        ArgumentNullException.ThrowIfNull(contactPreferences);

        return new PersonContactDetails
        {
            IsReserved = contactPreferences.IsReserved,
            EmailAddress = contactPreferences.Email,
            LanguageCode = contactPreferences.LanguageCode,
            MobilePhoneNumber = contactPreferences.MobileNumber,
            NationalIdentityNumber = contactPreferences.NationalIdentityNumber
        };
    }

    /// <summary>
    /// Maps the person contact details lookup result to a <see cref="PersonContactDetailsLookupResult"/>.
    /// </summary>
    /// <param name="lookupResult">The lookup result containing the person contact details.</param>
    /// <returns>
    /// A <see cref="Result{TValue, TError}"/> containing a <see cref="PersonContactDetailsLookupResult"/> if the mapping is successful, or <c>false</c> if the mapping fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lookupResult"/> is null.</exception>
    private Result<PersonContactDetailsLookupResult, bool> MapToContactDetailsLookupResult(IPersonContactPreferencesLookupResult lookupResult)
    {
        ArgumentNullException.ThrowIfNull(lookupResult);

        var matchedContactDetails = lookupResult.MatchedPersonContactPreferences?.Select(MapToContactDetails).ToImmutableList();

        return new PersonContactDetailsLookupResult(matchedContactDetails, lookupResult.UnmatchedNationalIdentityNumbers);
    }
}
