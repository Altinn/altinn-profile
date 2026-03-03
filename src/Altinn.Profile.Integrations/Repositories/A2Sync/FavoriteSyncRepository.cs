using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.PartyGroups;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Defines a repository for operations related to a users groups of parties.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public class FavoriteSyncRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry? telemetry) : IFavoriteSyncRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
        private readonly Telemetry? _telemetry = telemetry;

        /// <summary>
        /// Get a users favorite group.
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        internal async Task<Group?> GetFavorites(int userId, CancellationToken cancellationToken)
        {
            var groups = await GetGroups(userId, true, cancellationToken);

            var favorites = groups.FirstOrDefault();

            return favorites;
        }

        private async Task<List<Group>> GetGroups(int userId, bool filterOnlyFavorite, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var groups = await databaseContext.Groups.Include(g => g.Parties).Where(g => g.UserId == userId && (!filterOnlyFavorite || g.IsFavorite)).ToListAsync(cancellationToken);

            return groups;
        }

        /// <inheritdoc/>
        public async Task AddPartyToFavorites(int userId, Guid partyUuid, DateTime created, CancellationToken cancellationToken)
        {
            var favoriteGroup = await GetFavorites(userId, cancellationToken);
            if (favoriteGroup == null)
            {
                await CreateFavoriteGroupWithAssociation(userId, partyUuid, created, cancellationToken);
                return;
            }

            if (favoriteGroup.Parties.Any(p => p.PartyUuid == partyUuid))
            {
                return;
            }

            var partyGroupAssociation = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
                GroupId = favoriteGroup.GroupId,
                Created = created
            };
            favoriteGroup.Parties.Add(partyGroupAssociation);

            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            databaseContext.PartyGroupAssociations.Add(partyGroupAssociation);

            await databaseContext.SaveChangesAsync(cancellationToken);
            _telemetry?.FavoriteAdded();
        }

        private async Task<bool> CreateFavoriteGroupWithAssociation(int userId, Guid partyUuid, DateTime created, CancellationToken cancellationToken)
        {
            var partyGroupAssociation = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
                Created = created
            };

            var favoriteGroup = new Group
            {
                UserId = userId,
                IsFavorite = true,
                Name = PartyGroupConstants.DefaultFavoritesName,
                Parties = [partyGroupAssociation]
            };

            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
            databaseContext.Groups.Add(favoriteGroup);
            await databaseContext.SaveChangesAsync(cancellationToken);
            _telemetry?.FavoriteAdded();

            return true;
        }

        /// <inheritdoc/>
        public async Task DeleteFromFavorites(int userId, Guid partyUuid, DateTime deleted, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var favoriteGroup = await databaseContext.Groups.Include(g => g.Parties).Where(g => g.UserId == userId && g.IsFavorite).FirstOrDefaultAsync(cancellationToken);
            if (favoriteGroup == null)
            {
                return;
            }

            if (!favoriteGroup.Parties.Any(p => p.PartyUuid == partyUuid))
            {
                return;
            }

            var partyGroupAssociation = favoriteGroup.Parties.First(p => p.PartyUuid == partyUuid);
            if (partyGroupAssociation.Created > deleted)
            {
                return;
            }

            databaseContext.PartyGroupAssociations.Remove(partyGroupAssociation);
            await databaseContext.SaveChangesAsync(cancellationToken);

            _telemetry?.FavoriteDeleted();
        }
    }
}
