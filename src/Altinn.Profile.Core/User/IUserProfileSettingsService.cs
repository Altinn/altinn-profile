using System;
using System.Collections.Generic;
using System.Text;

using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User
{
    /// <summary>
    /// Provides services for enriching <see cref="UserProfile"/> instances with profile settings from the repository. Used to separate concerns of which part of the user profile that is cached. 
    /// </summary>
    public interface IUserProfileSettingsService
    {
        /// <summary>
        /// Enriches the given <see cref="UserProfile"/> with profile settings from the repository.
        /// </summary>
        /// <param name="userProfile">The userProfile to enrich.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<UserProfile> EnrichWithProfileSettings(UserProfile userProfile);
    }
}
