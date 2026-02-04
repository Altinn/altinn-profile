using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Altinn.Profile.Controllers;

/// <summary>
/// Represents the criteria used to look up contact details for self-identified users by their email identifiers.
/// </summary>
public class SiContactDetailsLookupCriteria
{
    /// <summary>
    /// A list of email identifiers for which to retrieve contact points for self-identified users.
    /// </summary>
    [JsonRequired]
    [Required]
    public List<string> EmailIdentifiers { get; set; } = [];
}
}
