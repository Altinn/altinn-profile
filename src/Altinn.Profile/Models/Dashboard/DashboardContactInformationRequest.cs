using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Models.Dashboard;

/// <summary>
/// Request model for getting contact information by National Identity Number
/// </summary>
public class DashboardContactInformationRequest
{
    /// <summary>
    /// The National Identity Number of the user to retrieve contact information for
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "The National Identity Number is not valid. It must contain exactly 11 digits")]
    [FromHeader]
    public string NationalIdentityNumber { get; set; } = string.Empty;
}
