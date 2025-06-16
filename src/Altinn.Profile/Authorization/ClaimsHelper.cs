using System.Linq;
using AltinnCore.Authentication.Constants;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Profile.Authorization
{
    /// <summary>
    /// Helper class for working with claims in the HTTP context.
    /// </summary>
    public class ClaimsHelper
    {
        /// <summary>
        /// Attempts to retrieve the user ID from the claims in the provided HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context containing the claims.</param>
        /// <param name="userId">The user ID retrieved from the claims, if successful.</param>
        /// <returns>
        /// A <see cref="BadRequestObjectResult"/> if the user ID is missing or invalid; otherwise, null.
        /// </returns>
        public static BadRequestObjectResult TryGetUserIdFromClaims(HttpContext context, out int userId)
        {
            userId = 0;
            string userIdString = context.User.Claims
                .Where(c => c.Type == AltinnCoreClaimTypes.UserId)
                .Select(c => c.Value).SingleOrDefault();

            if (string.IsNullOrEmpty(userIdString))
            {
                return new BadRequestObjectResult("Invalid request context. UserId must be provided in claims.");
            }

            if (!int.TryParse(userIdString, out userId))
            {
                return new BadRequestObjectResult("Invalid user ID format in claims.");
            }

            return null; // Success case
        }
    }
}
