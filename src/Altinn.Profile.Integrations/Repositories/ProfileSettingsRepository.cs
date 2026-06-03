using Altinn.Profile.Core.Integrations;
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
        public async Task<ProfileSettings> UpdateProfileSettings(ProfileSettings profileSettings, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await databaseContext.ProfileSettings
                .SingleOrDefaultAsync(g => g.UserId == profileSettings.UserId, cancellationToken);

            if (existing != null)
            {
                existing.UpdateFrom(profileSettings);

                await databaseContext.SaveChangesAsync(cancellationToken);

                return existing;
            }
            else
            {
                databaseContext.ProfileSettings.Add(profileSettings);

                await databaseContext.SaveChangesAsync(cancellationToken);

                return profileSettings;
            }
        }

        /// <inheritdoc/>
        public async Task<ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await databaseContext.ProfileSettings
                .SingleOrDefaultAsync(g => g.UserId == profileSettings.UserId, cancellationToken);

            if (existing == null)
            {
                // If there are no profile settings for the user, we initialize it with default values to ensure that the user profile always has valid profile settings.
                existing = ProfileSettings.GetDefaultValues();
                existing.UpdateFrom(profileSettings);
                existing.UserId = profileSettings.UserId;

                databaseContext.ProfileSettings.Add(existing);
            }
            else
            {
                existing.UpdateFrom(profileSettings);
            }

            await databaseContext.SaveChangesAsync(cancellationToken);

            return existing;
        }

        /// <inheritdoc/>
        public async Task<ProfileSettings?> GetProfileSettings(int userId, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await databaseContext.ProfileSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(g => g.UserId == userId, cancellationToken);
        }
    }
}
