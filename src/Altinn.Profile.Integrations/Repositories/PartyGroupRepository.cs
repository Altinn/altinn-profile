using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Defines a repository for operations related to a users groups of parties.
    /// </summary>
    public class PartyGroupRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IPartyGroupRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc />
        public async Task<int[]> GetFavorites(int userID, CancellationToken cancellationToken)
        {
            var groups = GetGroups(userID, true, cancellationToken);

            var favorites = groups.FirstOrDefault()?.Parties
                .Select(p => p.RegistryID)
                .ToArray();
            if (favorites == null)
            {
                return Array.Empty<int>();
            }

            return favorites;
        }

        public async Task<List<Group>> GetGroups(int userID, bool filterOnlyFavorite, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            var groups = await databaseContext.Groups.Include(g => g.Parties).Where(g => g.UserID == userID && (!filterOnlyFavorite || g.IsFavorite)).ToListAsync(cancellationToken);

            return groups;
        }
    }
}
