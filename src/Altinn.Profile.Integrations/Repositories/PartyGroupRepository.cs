using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Wolverine.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Defines a repository for operations related to a users groups of parties.
    /// </summary>
    public class PartyGroupRepository(
        IDbContextFactory<ProfileDbContext> contextFactory, IDbContextOutbox databaseContextOutbox) 
        : EFCoreTransactionalOutbox(databaseContextOutbox), IPartyGroupRepository
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
            var favoriteGroup = await GetFavorites(userId, cancellationToken);
            if (favoriteGroup == null)
            {
                return await CreateFavoriteGroupWithAssociation(userId, partyUuid, cancellationToken);
            }

            if (favoriteGroup.Parties.Any(p => p.PartyUuid == partyUuid))
            {
                return false;
            }

            var partyGroupAssociation = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
                GroupId = favoriteGroup.GroupId,
            };
            favoriteGroup.Parties.Add(partyGroupAssociation);

            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            databaseContext.PartyGroupAssociations.Add(partyGroupAssociation);

            FavoriteAddedEvent NotifyFavoriteAdded() => new(userId, partyUuid, RegistrationTimestamp: DateTime.Now);

            await NotifyAndSave(databaseContext, NotifyFavoriteAdded, cancellationToken);

            return true;
        }
        
        private async Task<bool> CreateFavoriteGroupWithAssociation(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            var partyGroupAssociation = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
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

            FavoriteAddedEvent NotifyFavoriteAdded() => new(userId, partyUuid, RegistrationTimestamp: DateTime.Now);

            await NotifyAndSave(databaseContext, eventRaiser: NotifyFavoriteAdded, cancellationToken);
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFromFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var favoriteGroup = await databaseContext.Groups.Include(g => g.Parties).Where(g => g.UserId == userId && g.IsFavorite).FirstOrDefaultAsync(cancellationToken);
            if (favoriteGroup == null)
            {
                return false;
            }

            if (!favoriteGroup.Parties.Any(p => p.PartyUuid == partyUuid))
            {
                return false;
            }

            var partyGroupAssociation = favoriteGroup.Parties.First(p => p.PartyUuid == partyUuid);

            databaseContext.PartyGroupAssociations.Remove(partyGroupAssociation);

            FavoriteRemovedEvent NotifyFavoriteDeleted() => new(userId, partyUuid, partyGroupAssociation.Created);
            await NotifyAndSave(databaseContext, eventRaiser: NotifyFavoriteDeleted, cancellationToken);

            return true;
        }
    }
}
