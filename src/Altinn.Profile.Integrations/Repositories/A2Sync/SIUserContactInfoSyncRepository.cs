using System;
using System.Collections.Generic;
using System.Text;

using Altinn.Profile.Core.Integrations;
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
    public class SIUserContactInfoSyncRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry telemetry) : ISIUserContactInfoSyncRepository
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
                existingUser.EmailAddress = userContactSettings.EmailAddress ?? string.Empty;
                existingUser.PhoneNumber = userContactSettings.PhoneNumber;
                existingUser.PhoneNumberLastChanged = string.IsNullOrWhiteSpace(userContactSettings.PhoneNumber) ? null : updatedDatetime;
                databaseContext.SelfIdentifiedUsers.Update(existingUser);
                await databaseContext.SaveChangesAsync(cancellationToken);
                _telemetry?.SiUserContactSettingsUpdated();

                return existingUser;
            }

            var currentDateTime = DateTime.UtcNow;

            var userContactInfo = new UserContactInfo()
            {
                CreatedAt = updatedDatetime,
                UserId = userContactSettings.UserId,
                UserUuid = userContactSettings.UserUuid,
                Username = userContactSettings.UserName,
                EmailAddress = userContactSettings.EmailAddress ?? string.Empty,
                PhoneNumber = userContactSettings.PhoneNumber,
                PhoneNumberLastChanged = string.IsNullOrWhiteSpace(userContactSettings.PhoneNumber) ? null : updatedDatetime
            };

            databaseContext.SelfIdentifiedUsers.Add(userContactInfo);
            await databaseContext.SaveChangesAsync(cancellationToken);

            return userContactInfo;
        }
    }
}
