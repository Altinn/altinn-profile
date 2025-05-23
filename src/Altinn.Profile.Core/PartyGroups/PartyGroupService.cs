using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.PartyGroups
{
    /// <inheritdoc/>
    public class PartyGroupService(IPartyGroupRepository groupRepository) : IPartyGroupService
    {
        private readonly IPartyGroupRepository _groupRepository = groupRepository;
        private const string DefaultFavoritesName = "__favoritter__";

        /// <inheritdoc/>
        public async Task<Group> GetFavorites(int userId, CancellationToken cancellationToken)
        {
            var favorites = await _groupRepository.GetFavorites(userId, cancellationToken);

            return favorites ?? new Group
            {
                Parties = [],
                Name = DefaultFavoritesName,
                IsFavorite = true
            };  
        }
    }
}
