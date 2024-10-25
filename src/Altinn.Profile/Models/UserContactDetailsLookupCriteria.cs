using System.Collections.Generic;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the lookup criteria to retrieve the contact details for one or more persons.
/// </summary>
public class UserContactDetailsLookupCriteria
{
    /// <summary>
    /// A collection of national identity numbers used to retrieve contact points, obtain contact details, or check the availability of contact points.
    /// </summary>
    public List<string> NationalIdentityNumbers { get; set; } = [];
}
