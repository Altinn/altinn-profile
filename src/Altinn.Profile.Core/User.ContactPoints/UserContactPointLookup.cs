namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// A class respresenting a user contact point lookup object
/// </summary>
public class UserContactPointLookup
{
    /// <summary>
    /// A list of national identity numbers to look up contact points or contact point availability for
    /// </summary>
    public List<string> NationalIdentityNumbers { get; set; } = [];
}
