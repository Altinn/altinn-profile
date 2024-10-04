using System.Collections.Generic;

namespace Altinn.Profile.Models;

/// <summary>
/// A class representing a user contact point lookup object.
/// </summary>
public class UserContactPointLookup
{
    /// <summary>
    /// A collection of national identity numbers used to retrieve contact points or check contact point availability.
    /// </summary>
    public List<string> NationalIdentityNumbers { get; set; } = [];
}
