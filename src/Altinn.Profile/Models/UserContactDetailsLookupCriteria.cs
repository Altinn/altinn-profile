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

    /// <summary>
    /// Indicates whether the lookup should skip the age check for contact points. If set to true, 
    /// contact points will be returned regardless of their age.
    /// </summary>
    public bool UseStaleContactInfo { get; set; } = false;
}
