using System.Collections.Immutable;

using Altinn.Profile.Core.Extensions;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Implementation of the national identity number checking service.
/// </summary>
public class NationalIdentityNumberChecker : INationalIdentityNumberChecker
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public (IImmutableList<string> Valid, IImmutableList<string> Invalid) Categorize(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        var validNationalIdentityNumbers = nationalIdentityNumbers.Where(IsValid).ToImmutableList();
        var invalidNationalIdentityNumbers = nationalIdentityNumbers.Except(validNationalIdentityNumbers).ToImmutableList();

        return (validNationalIdentityNumbers, invalidNationalIdentityNumbers);
    }

    /// <summary>
    /// Validates a collection of national identity numbers and returns the valid ones only.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// An immutable list of valid national identity numbers.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public IImmutableList<string> GetValid(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        return nationalIdentityNumbers.Where(IsValid).ToImmutableList();
    }

    /// <summary>
    /// Checks the validity of a single national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns>
    /// <c>true</c> if the national identity number is valid; otherwise, <c>false</c>.
    /// </returns>
    public bool IsValid(string nationalIdentityNumber)
    {
        return nationalIdentityNumber.IsValidSocialSecurityNumber();
    }
}
