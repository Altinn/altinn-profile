using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <inheritdoc/>
    public class ProfessionalNotificationsRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IProfessionalNotificationsRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc/>
        public async Task<UserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var userPartyContactInfo = await databaseContext.UserPartyContactInfo
                .AsNoTracking()
                .Include(g => g.UserPartyContactInfoResources)
                .FirstOrDefaultAsync(g => g.UserId == userId && g.PartyUuid == partyUuid, cancellationToken);

            return userPartyContactInfo;
        }

        /// <inheritdoc/>
        public async Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await databaseContext.UserPartyContactInfo
                .Include(g => g.UserPartyContactInfoResources)
                .FirstOrDefaultAsync(g => g.UserId == contactInfo.UserId && g.PartyUuid == contactInfo.PartyUuid, cancellationToken);

            bool wasAdded;
            if (existing == null)
            {
                databaseContext.UserPartyContactInfo.Add(contactInfo);
                wasAdded = true;
            }
            else
            {
                existing.EmailAddress = contactInfo.EmailAddress;
                existing.PhoneNumber = contactInfo.PhoneNumber;

                // Synchronize the UserPartyContactInfoResources collection
                var incomingResourceIds = contactInfo.UserPartyContactInfoResources?.Select(r => r.ResourceId).ToHashSet() ?? new HashSet<string>();
                var existingResourceIds = existing.UserPartyContactInfoResources?.Select(r => r.ResourceId).ToHashSet() ?? new HashSet<string>();

                // Remove resources that are no longer present
                if (existing.UserPartyContactInfoResources?.Count > 0)
                {
                    var resourcesToRemove = existing.UserPartyContactInfoResources
                        .Where(r => !incomingResourceIds.Contains(r.ResourceId)).ToList();

                    foreach (var resourceToRemove in resourcesToRemove)
                    {
                        existing.UserPartyContactInfoResources.Remove(resourceToRemove);
                    }
                }

                // Add new resources that don't exist yet
                if (contactInfo.UserPartyContactInfoResources?.Count > 0)
                {
                    existing.UserPartyContactInfoResources ??= [];

                    var newResources = contactInfo.UserPartyContactInfoResources
                        .Where(r => !existingResourceIds.Contains(r.ResourceId));

                    foreach (var newResource in newResources)
                    {
                        existing.UserPartyContactInfoResources.Add(new UserPartyContactInfoResource
                        {
                            ResourceId = newResource.ResourceId,
                            UserPartyContactInfoId = existing.UserPartyContactInfoId
                        });
                    }
                }

                databaseContext.UserPartyContactInfo.Update(existing);
                wasAdded = false;
            }

            await databaseContext.SaveChangesAsync(cancellationToken);

            return wasAdded;
        }
    }
}
