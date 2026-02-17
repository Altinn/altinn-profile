using Altinn.Urn;

namespace Altinn.Profile.Core.Unit.ContactPoints;

/// <summary>
/// Represents a URN value that may encode an ID-porten email address, providing methods to identify and extract the
/// email if present.
/// </summary>
/// <remarks>Use the IsIDPortenEmail method to determine whether the URN contains a valid ID-porten email address
/// and to retrieve the decoded email value. This type is typically used in scenarios where URN-based identification is
/// required, such as authentication or user mapping. Thread safety depends on the underlying implementation of the
/// record and its members.</remarks>
[KeyValueUrn]
public abstract partial record CustomContactPointUrn
{
    /// <summary>
    /// Determines whether the current value represents an ID-porten email address and retrieves the decoded email if
    /// available.
    /// </summary>
    /// <param name="email">When this method returns, contains the decoded email address if the value is a valid ID-porten email; otherwise,
    /// contains the default value for <see cref="UrnEncoded"/>.</param>
    /// <returns>true if the value is a valid ID-porten email address; otherwise, false.</returns>
    [UrnKey("altinn:person:idporten-email")]
    public partial bool IsIDPortenEmail(out UrnEncoded email);

    /// <summary>
    /// Determines whether the current value represents legacy username SI user and retrieves the decoded username if available.
    /// </summary>
    /// <param name="username">When this method returns, contains the decoded username if the value is a valid username; otherwise,
    /// contains the default value for <see cref="UrnEncoded"/>.</param>
    /// <returns>true if the value is a valid legacy username; otherwise, false.</returns>
    [UrnKey("altinn:party:username", Canonical = true)]
    [UrnKey("altinn:username")]
    [UrnKey("altinn:person:legacy-selfidentified")]
    public partial bool IsUsername(out UrnEncoded username);
}
