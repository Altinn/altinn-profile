﻿namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// Represents a set of contact points for a self-identified user, including mobile number and email address.
/// </summary>
public class SiUserContactPoints
{
    /// <summary>
    /// The mobile number associated with the self-identified user
    /// </summary>
    public string? MobileNumber { get; init; }

    /// <summary>
    /// The email address associated with the self-identified user
    /// </summary>
    public required string Email { get; init; }
}

/// <summary>
/// A list representation of <see cref="SiUserContactPoints"/>
/// </summary>
public class SelfIdentifiedUserContactPointsList
{
    /// <summary>
    /// A list containing contact points for self-identified users
    /// </summary>
    public List<SiUserContactPoints> ContactPointsList { get; set; } = [];
}
