using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.PartyGroups
{
    /// <inheritdoc/>
    public class PartyGroupService(IPartyGroupRepository groupRepository) : IPartyGroupService
    {
        private readonly IPartyGroupRepository _groupRepository = groupRepository;

        /// <inheritdoc/>
        public async Task<Group> GetFavorites(int userId, CancellationToken cancellationToken)
        {
            var favorites = await _groupRepository.GetFavorites(userId, cancellationToken);

            return favorites ?? new Group
            {
                Parties = [],
                Name = PartyGroupConstants.DefaultFavoritesName,
                IsFavorite = true
            };  
        }

        /// <inheritdoc/>
        public async Task MarkPartyAsFavorite(int userId, int partyId, CancellationToken cancellationToken)
        {
            await _groupRepository.AddPartyToFavorites(userId, partyId, cancellationToken);
        }
    }
}
