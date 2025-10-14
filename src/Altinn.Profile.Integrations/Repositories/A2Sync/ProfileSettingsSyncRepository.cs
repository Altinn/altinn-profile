using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Repository for synchronizing profile settings with Altinn2
    /// </summary>
    public class ProfileSettingsSyncRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry? telemetry) : IProfileSettingsSyncRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
        private readonly Telemetry? _telemetry = telemetry;

        /// <inheritdoc/>
        public async Task UpdateProfileSettings(ProfileSettings profileSettings)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            var existing = await databaseContext.ProfileSettings
                .FirstOrDefaultAsync(g => g.UserId == profileSettings.UserId);

            if (existing != null)
            {
                existing.DoNotPromptForParty = profileSettings.DoNotPromptForParty;
                existing.PreselectedPartyUuid = profileSettings.PreselectedPartyUuid;
                existing.ShowClientUnits = profileSettings.ShowClientUnits;
                existing.ShouldShowSubEntities = profileSettings.ShouldShowSubEntities;
                existing.ShouldShowDeletedEntities = profileSettings.ShouldShowDeletedEntities;
                existing.IgnoreUnitProfileDateTime = profileSettings.IgnoreUnitProfileDateTime;
                existing.LanguageType = profileSettings.LanguageType;

                await databaseContext.SaveChangesAsync();
                _telemetry?.ProfileSettingsUpdated();
            }
            else
            {
                databaseContext.ProfileSettings.Add(profileSettings);
                await databaseContext.SaveChangesAsync();
                _telemetry?.ProfileSettingsAdded();
            }
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
