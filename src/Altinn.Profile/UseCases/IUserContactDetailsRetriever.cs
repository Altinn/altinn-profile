using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Models;

namespace Altinn.Profile.UseCases;

/// <summary>
/// Defines a use case for retrieving user contact details.
/// </summary>
public interface IUserContactDetailsRetriever
{
    /// <summary>
    /// Asynchronously retrieves the contact details for one or more users based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookupCriteria">The user contact point lookup criteria, which includes national identity numbers.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the outcome of the user contact details retrieval.</returns>
    Task<Result<UserContactDetailsLookupResult, bool>> RetrieveAsync(UserContactPointLookup lookupCriteria);
}
