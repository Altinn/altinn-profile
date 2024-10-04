using System.Collections.Immutable;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Provides functionality for checking the validity of national identity numbers.
/// </summary>
public interface INationalIdentityNumberChecker
{
    /// <summary>
    /// Validates a collection of national identity numbers and categorizes them into valid and invalid groups.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A tuple containing two immutable lists: 
    /// <list type="bullet">
    /// <item><description><c>Valid</c>: An immutable list of valid national identity numbers.</description></item>
    /// <item><description><c>Invalid</c>: An immutable list of invalid national identity numbers.</description></item>
    /// </list>
    /// </returns>
    (IImmutableList<string> Valid, IImmutableList<string> Invalid) Categorize(IEnumerable<string> nationalIdentityNumbers);

    /// <summary>
    /// Checks the validity of a single national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns>
    /// <c>true</c> if the national identity number is valid; otherwise, <c>false</c>.
    /// </returns>
    bool IsValid(string nationalIdentityNumber);
}
