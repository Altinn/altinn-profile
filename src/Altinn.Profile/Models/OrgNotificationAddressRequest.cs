using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// A class describing the query model for contact points for organizations
    /// </summary>
    public class OrgNotificationAddressRequest: IValidatableObject
    {
        /// <summary>
        /// Gets or sets the list of organization numbers to lookup contact points for
        /// </summary>
        [JsonPropertyName("organizationNumbers")]
        [Required]
        public List<string> OrganizationNumbers { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrganizationNumbers == null ||
                OrganizationNumbers.Count == 0 ||
                OrganizationNumbers.Any(string.IsNullOrWhiteSpace))
            {
                yield return new ValidationResult("OrganizationNumbers must contain a list of valid organization number values", [nameof(OrganizationNumbers)]);
            }
        }
    }
}
