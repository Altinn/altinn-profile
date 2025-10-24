using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Repository for updating profile settings
    /// </summary>
    public class ProfileSettingsRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IProfileSettingsRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc/>
        public async Task<ProfileSettings> UpdateProfileSettings(ProfileSettings profileSettings)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            var existing = await databaseContext.ProfileSettings
                .FirstOrDefaultAsync(g => g.UserId == profileSettings.UserId);

            if (existing != null)
            {
                existing.UpdateFrom(profileSettings);

                await databaseContext.SaveChangesAsync();
                return existing;
            }
            else
            {
                databaseContext.ProfileSettings.Add(profileSettings);
                await databaseContext.SaveChangesAsync();
                return profileSettings;
            }
        }

        /// <inheritdoc/>
        public async Task<ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchRequest profileSettings)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            var existing = await databaseContext.ProfileSettings
                .FirstOrDefaultAsync(g => g.UserId == profileSettings.UserId);

            if (existing == null)
            {
                return null;
            }

            existing.UpdateFrom(profileSettings);

            await databaseContext.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Retrieves the profile settings for a given user ID.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ProfileSettings?> GetProfileSettings(int userId)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            return await databaseContext.ProfileSettings
                .FirstOrDefaultAsync(g => g.UserId == userId);
        }
    }
}
