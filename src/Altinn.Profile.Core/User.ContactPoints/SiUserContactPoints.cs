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

    /// <summary>
    /// The user id associated with the self-identified user
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The user uuid associated with the self-identified user
    /// </summary>
    public Guid UserUuid { get; init; }

    /// <summary>
    /// The party id associated with the self-identified user
    /// </summary>
    public int PartyId { get; init; }

    /// <summary>
    /// The party uuid associated with the self-identified user
    /// </summary>
    public Guid PartyUuid { get; init; }
}

/// <summary>
/// A list representation of <see cref="SiUserContactPoints"/>
/// </summary>
public class SiUserContactPointsList
{
    /// <summary>
    /// A list containing contact points for self-identified users
    /// </summary>
    public List<SiUserContactPoints> ContactPointsList { get; set; } = [];
}
