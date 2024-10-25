using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Models;

namespace Altinn.Profile.UseCases;

/// <summary>
/// Defines a use case for retrieving the contact details for one or more persons.
/// </summary>
public interface IPersonContactDetailsRetriever
{
    /// <summary>
    /// Asynchronously retrieves the contact details for one or more persons based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookupCriteria">The criteria used to look up contact .</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="PersonContactDetailsLookupResult"/> represents the successful outcome and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<PersonContactDetailsLookupResult, bool>> RetrieveAsync(UserContactDetailsLookupCriteria lookupCriteria);
}
