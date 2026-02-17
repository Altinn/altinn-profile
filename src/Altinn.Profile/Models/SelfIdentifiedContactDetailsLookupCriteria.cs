using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models;

/// <summary>
/// Represents the criteria used to look up contact details for self-identified users by their external identities.
/// </summary>
public class SelfIdentifiedContactDetailsLookupCriteria
{
    /// <summary>
    /// A list of external identities for which to retrieve contact points for self-identified users.
    /// </summary>
    [JsonRequired]
    [Required]
    public List<string> ExternalIdentities { get; set; } = [];
}
