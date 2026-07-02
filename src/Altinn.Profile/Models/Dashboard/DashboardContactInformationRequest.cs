using System.ComponentModel.DataAnnotations;

namespace Altinn.Profile.Models.Dashboard;

/// <summary>
/// Request model for getting contact information by SSN
/// </summary>
public class DashboardContactInformationRequest
{
    /// <summary>
    /// The social security number (SSN) of the user to retrieve contact information for
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "The SSN is not valid. It must contain exactly 11 digits")]
    public string Ssn { get; set; } = string.Empty;
}
