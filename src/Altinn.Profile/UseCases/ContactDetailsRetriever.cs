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
/// Provides an implementation for retrieving the contact details for one or more persons.
/// </summary>
public class ContactDetailsRetriever : IContactDetailsRetriever
{
    private readonly IPersonService _personService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactDetailsRetriever"/> class.
    /// </summary>
    /// <param name="personService">The person service for retrieving contact details.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="personService"/> is null.</exception>
    public ContactDetailsRetriever(IPersonService personService)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
    }

    /// <summary>
    /// Asynchronously retrieves the contact details for one or more persons based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookupCriteria">The criteria used to look up contact details, including the national identity numbers of the persons.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="ContactDetailsLookupResult"/> represents the successful outcome and <see cref="bool"/> indicates a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lookupCriteria"/> is null.</exception>
    public async Task<Result<ContactDetailsLookupResult, bool>> RetrieveAsync(UserContactPointLookup lookupCriteria)
    {
        ArgumentNullException.ThrowIfNull(lookupCriteria);

        if (lookupCriteria.NationalIdentityNumbers == null || lookupCriteria.NationalIdentityNumbers.Count == 0)
        {
            return false;
        }

        var contactDetails = await _personService.GetContactDetailsAsync(lookupCriteria.NationalIdentityNumbers);

        return contactDetails.Match(
            MapToContactDetailsLookupResult,
            _ => false);
    }

    /// <summary>
    /// Maps the person contact details to a <see cref="ContactDetails"/>.
    /// </summary>
    /// <param name="contactDetails">The person contact details to map.</param>
    /// <returns>The mapped <see cref="ContactDetails"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contactDetails"/> is null.</exception>
    private ContactDetails MapToContactDetails(IPersonContactDetails contactDetails)
    {
        ArgumentNullException.ThrowIfNull(contactDetails);

        return new ContactDetails
        {
            Reservation = contactDetails.IsReserved,
            EmailAddress = contactDetails.EmailAddress,
            LanguageCode = contactDetails.LanguageCode,
            MobilePhoneNumber = contactDetails.MobilePhoneNumber,
            NationalIdentityNumber = contactDetails.NationalIdentityNumber
        };
    }

    /// <summary>
    /// Maps the person contact details lookup result to a <see cref="ContactDetailsLookupResult"/>.
    /// </summary>
    /// <param name="lookupResult">The lookup result containing the person contact details.</param>
    /// <returns>
    /// A <see cref="Result{TValue, TError}"/> containing a <see cref="ContactDetailsLookupResult"/> if the mapping is successful, or <c>false</c> if the mapping fails.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lookupResult"/> is null.</exception>
    private Result<ContactDetailsLookupResult, bool> MapToContactDetailsLookupResult(IPersonContactDetailsLookupResult lookupResult)
    {
        ArgumentNullException.ThrowIfNull(lookupResult);

        var matchedContactDetails = lookupResult.MatchedPersonContactDetails?.Select(MapToContactDetails).ToImmutableList();

        return new ContactDetailsLookupResult(matchedContactDetails, lookupResult.UnmatchedNationalIdentityNumbers);
    }
}
