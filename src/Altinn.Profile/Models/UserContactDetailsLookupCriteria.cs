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
    /// Indicates whether the lookup should ignore the notification status of the contact points. If set to true, 
    /// contact points will be returned regardless of their notification status.
    /// Notification status is determined by the age of the contact point or time since last verification.
    /// </summary>
    public bool IgnoreNotificationStatus { get; set; } = false;
}
