using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Wolverine.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Repository for updating profile settings
    /// </summary>
    public class ProfileSettingsRepository(IDbContextFactory<ProfileDbContext> contextFactory, IDbContextOutbox databaseContextOutbox) : EFCoreTransactionalOutbox(databaseContextOutbox), IProfileSettingsRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc/>
        public async Task<ProfileSettings> UpdateProfileSettings(ProfileSettings profileSettings, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await databaseContext.ProfileSettings
                .FirstOrDefaultAsync(g => g.UserId == profileSettings.UserId, cancellationToken);

            if (existing != null)
            {
                existing.UpdateFrom(profileSettings);

                ProfileSettingsUpdatedEvent NotifyProfileSettingsUpdated() => new(profileSettings.UserId, DateTime.UtcNow, existing.LanguageType, existing.DoNotPromptForParty, existing.PreselectedPartyUuid, existing.ShowClientUnits, existing.ShouldShowSubEntities, existing.ShouldShowDeletedEntities, existing.IgnoreUnitProfileDateTime);
                await NotifyAndSave(databaseContext, NotifyProfileSettingsUpdated, cancellationToken);

                return existing;
            }
            else
            {
                databaseContext.ProfileSettings.Add(profileSettings);

                ProfileSettingsUpdatedEvent NotifyProfileSettingsUpdated() => new(profileSettings.UserId, DateTime.UtcNow, profileSettings.LanguageType, profileSettings.DoNotPromptForParty, profileSettings.PreselectedPartyUuid, profileSettings.ShowClientUnits, profileSettings.ShouldShowSubEntities, profileSettings.ShouldShowDeletedEntities, profileSettings.IgnoreUnitProfileDateTime);
                await NotifyAndSave(databaseContext, NotifyProfileSettingsUpdated, cancellationToken);

                return profileSettings;
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
