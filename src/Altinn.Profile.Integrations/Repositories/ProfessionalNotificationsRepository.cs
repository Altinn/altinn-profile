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
        public async Task<UserPartyContactInfo?> GetNotificationAddresses(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var userPartyContactInfo = await databaseContext.UserPartyContactInfo
                .AsNoTracking()
                .Include(g => g.UserPartyContactInfoResources)
                .FirstOrDefaultAsync(g => g.UserId == userId && g.PartyUuid == partyUuid, cancellationToken);

            return userPartyContactInfo;
        }
    }
}
