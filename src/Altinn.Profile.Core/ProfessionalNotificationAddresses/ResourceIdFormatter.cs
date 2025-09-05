using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Provides methods for formatting and sanitizing resource IDs for professional notification addresses.
    /// </summary>
    public class ResourceIdFormatter
    {
        private readonly static string Prefix = "urn:altinn:resource:";

        /// <summary>
        /// Removes the standard resource prefix from the given resource ID, if present, and trims whitespace.
        /// Returns an empty string if the input is null or whitespace.
        /// </summary>
        /// <param name="resourceId">The resource ID to sanitize.</param>
        /// <returns>The sanitized resource ID without the prefix, or an empty string if input is null or whitespace.</returns>
        public static string GetSanitizedResourceId(string? resourceId)
        {
            var trimmedResourceId = resourceId?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedResourceId))
            {
                return string.Empty;
            }

            return trimmedResourceId.StartsWith(Prefix, StringComparison.Ordinal) ? trimmedResourceId[Prefix.Length..] : trimmedResourceId;
        }

        /// <summary>
        /// Adds the standard resource prefix to the given resource ID if it is not already present, and trims whitespace.
        /// Returns an empty string if the input is null or whitespace.
        /// </summary>
        /// <param name="resourceId">The resource ID to which the prefix should be added.</param>
        /// <returns>The resource ID with the standard prefix, or an empty string if input is null or whitespace.</returns>
        public static string AddPrefixToResourceId(string resourceId)
        {
            var trimmedResourceId = resourceId.Trim();

            if (string.IsNullOrWhiteSpace(trimmedResourceId))
            {
                return string.Empty;
            }

            return trimmedResourceId.StartsWith(Prefix, StringComparison.Ordinal) ? trimmedResourceId : $"{Prefix}{trimmedResourceId}";
        }    }
}
