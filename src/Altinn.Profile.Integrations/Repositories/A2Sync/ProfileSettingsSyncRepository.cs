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
    }
}
