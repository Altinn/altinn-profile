using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.PartyGroups;
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
        public async Task<Group?> GetFavorites(int userId, CancellationToken cancellationToken)
        {
            var groups = await GetGroups(userId, true, cancellationToken);

            var favorites = groups.FirstOrDefault();

            return favorites;
        }

        /// <inheritdoc/>
        public async Task<List<Group>> GetGroups(int userId, bool filterOnlyFavorite, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var groups = await databaseContext.Groups.Include(g => g.Parties).Where(g => g.UserId == userId && (!filterOnlyFavorite || g.IsFavorite)).ToListAsync(cancellationToken);

            return groups;
        }

        /// <inheritdoc/>
        public async Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            var favoriteGroup = await GetFavorites(userId, cancellationToken) ?? new Group
                {
                    UserId = userId,
                    IsFavorite = true,
                    Name = PartyGroupConstants.DefaultFavoritesName,
                    Parties = []
                };

            if (favoriteGroup.Parties.Any(p => p.PartyUuid == partyUuid))
            {
                return false;
            }

            var partyGroupAssociation = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
            };
            favoriteGroup.Parties.Add(partyGroupAssociation);

            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            bool isNewGroup = favoriteGroup.GroupId == 0;
            if (isNewGroup)
            {
               databaseContext.Groups.Add(favoriteGroup);
            }
            else
            {
               databaseContext.Groups.Update(favoriteGroup);
            }

            await databaseContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
