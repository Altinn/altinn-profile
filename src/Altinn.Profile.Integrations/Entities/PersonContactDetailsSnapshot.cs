﻿using System;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents the contact details of a person from the change log.
/// </summary>
public class PersonContactDetailsSnapshot : IPersonContactDetailsSnapshot
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    [JsonPropertyName("epostadresse")]
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Gets the date and time when the person's email address was updated.
    /// </summary>
    [JsonPropertyName("epostadresse_oppdatert")]
    public DateTime? EmailAddressUpdated { get; init; }

    /// <summary>
    /// Gets the date and time when the person's email address was last verified.
    /// </summary>
    [JsonPropertyName("epostadresse_sist_verifisert")]
    public DateTime? EmailAddressLastVerified { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person's email address is duplicated.
    /// </summary>
    [JsonPropertyName("epostadresse_duplisert")]
    public string? IsEmailAddressDuplicated { get; init; }

    /// <summary>
    /// Gets the mobile phone number of the person.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer")]
    public string? MobilePhoneNumber { get; init; }

    /// <summary>
    /// Gets the date and time when the person's mobile phone number was updated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_oppdatert")]
    public DateTime? MobilePhoneNumberUpdated { get; init; }

    /// <summary>
    /// Gets the date and time when the person's mobile phone number was last verified.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_sist_verifisert")]
    public DateTime? MobilePhoneNumberLastVerified { get; init; }

    /// <summary>
    /// Gets a value indicating whether the person's mobile phone number is duplicated.
    /// </summary>
    [JsonPropertyName("mobiltelefonnummer_duplisert")]
    public string? IsMobilePhoneNumberDuplicated { get; init; }
}