using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Repository for synchronizing self-identified user contact information from Altinn 2.
    /// </summary>
    public class SIUserContactInfoSyncRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry? telemetry = null) : ISIUserContactInfoSyncRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
        private readonly Telemetry? _telemetry = telemetry;

        /// <inheritdoc/>
        public async Task<UserContactInfo> InsertOrUpdate(SiUserContactSettings userContactSettings, DateTime updatedDatetime, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existingUser = await databaseContext.SelfIdentifiedUsers.FirstOrDefaultAsync(u => u.UserId == userContactSettings.UserId, cancellationToken);
            if (existingUser != null)
            {
                var phoneNumberUpdated = existingUser.PhoneNumber != userContactSettings.PhoneNumber;

                existingUser.EmailAddress = userContactSettings.EmailAddress;
                existingUser.PhoneNumber = userContactSettings.PhoneNumber;
                existingUser.PhoneNumberLastChanged = phoneNumberUpdated ? updatedDatetime : existingUser.PhoneNumberLastChanged;
                await databaseContext.SaveChangesAsync(cancellationToken);
                _telemetry?.SiUserContactSettingsUpdated();

                return existingUser;
            }

            var userContactInfo = new UserContactInfo()
            {
                CreatedAt = updatedDatetime,
                UserId = userContactSettings.UserId,
                UserUuid = userContactSettings.UserUuid,
                Username = userContactSettings.UserName,
                EmailAddress = userContactSettings.EmailAddress,
                PhoneNumber = userContactSettings.PhoneNumber,
                PhoneNumberLastChanged = string.IsNullOrWhiteSpace(userContactSettings.PhoneNumber) ? null : updatedDatetime
            };

            databaseContext.SelfIdentifiedUsers.Add(userContactInfo);
            await databaseContext.SaveChangesAsync(cancellationToken);
            _telemetry?.SiUserContactSettingsAdded();

            return userContactInfo;
        }
    }
}
