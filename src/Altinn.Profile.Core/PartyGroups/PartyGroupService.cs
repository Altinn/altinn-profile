using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.PartyGroups
{
    /// <inheritdoc/>
    public class PartyGroupService(IPartyGroupRepository groupRepository) : IPartyGroupService
    {
        private readonly IPartyGroupRepository _groupRepository = groupRepository;

        /// <inheritdoc/>
        public async Task<int[]> GetFavorites(int userId, CancellationToken cancellationToken)
        {
            return await _groupRepository.GetFavorites(userId, cancellationToken);
        }
    }
}
