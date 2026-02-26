using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.PartyGroups
{
    /// <inheritdoc/>
    public class PartyGroupService(IPartyGroupRepository groupRepository) : IPartyGroupService
    {
        private readonly IPartyGroupRepository _groupRepository = groupRepository;

        /// <inheritdoc/>
        public async Task<Group?> GetGroup(int userId, int groupId, CancellationToken cancellationToken)
        {
            var group = await _groupRepository.GetGroup(userId, groupId, cancellationToken);

            return group;
        }

        /// <inheritdoc/>
        public async Task<List<Group>> GetGroupsForAUser(int userId, CancellationToken cancellationToken)
        {
            var groups = await _groupRepository.GetGroups(userId, false, cancellationToken) ?? [];            
            
            return groups;
        }

        /// <inheritdoc/>
        public async Task<Group> CreateGroup(int userId, string name, CancellationToken cancellationToken)
        {
            var group = await _groupRepository.CreateGroup(userId, name, cancellationToken);

            return group;
        }

        /// <inheritdoc/>
        public async Task<UpdateGroupResult> UpdateGroupName(int userId, int groupId, string name, CancellationToken cancellationToken)
        {
            var result = await _groupRepository.UpdateGroupName(userId, groupId, name, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<GroupOperationResult> DeleteGroup(int userId, int groupId, CancellationToken cancellationToken)
        {
            var result = await _groupRepository.DeleteGroup(userId, groupId, cancellationToken);

            return result;
        }

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
        public async Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return await _groupRepository.AddPartyToFavorites(userId, partyUuid, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFromFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return await _groupRepository.DeleteFromFavorites(userId, partyUuid, cancellationToken);
        }
    }
}
