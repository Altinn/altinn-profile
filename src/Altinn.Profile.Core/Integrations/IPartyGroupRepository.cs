using Altinn.Profile.Core.User.PartyGroups;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface to interact with the party group repository
    /// </summary>
    public interface IPartyGroupRepository
    {
        /// <summary>
        /// Gets the favorite parties for a given user
        /// </summary>
        Task<Group?> GetFavorites(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a group for a given user and group id
        /// </summary>
        /// <param name="userId">The logged in users userId</param>
        /// <param name="groupId">The group id for the group to fetch</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/> with the group as a result.</returns>
        Task<Group?> GetGroup(int userId, int groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the groups for a given user.
        /// </summary>
        /// <param name="userId">The logged in users userId</param>
        /// <param name="filterOnlyFavorite">A flag to indicate that ionly the favorite group should be fetched</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/> with the groups as a result.</returns>
        Task<List<Group>> GetGroups(int userId, bool filterOnlyFavorite, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new group with the specified name and associates it with the given user.
        /// </summary>
        /// <param name="userId">The identifier of the user who will own or create the group. Must be a valid user ID.</param>
        /// <param name="name">The name of the group to create. Cannot be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created group.</returns>
        Task<Group> CreateGroup(int userId, string name, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the name of an existing group.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the group. Must be a valid user ID.</param>
        /// <param name="groupId">The identifier of the group to update.</param>
        /// <param name="name">The new name for the group. Cannot be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the operation result and the updated group if successful.</returns>
        Task<UpdateGroupResult> UpdateGroupName(int userId, int groupId, string name, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a group for a given user.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the group. Must be a valid user ID.</param>
        /// <param name="groupId">The identifier of the group to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates the operation outcome.</returns>
        Task<GroupOperationResult> DeleteGroup(int userId, int groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a party to a group.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the group. Must be a valid user ID.</param>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="partyUuid">The UUID of the party to add to the group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated group, or null if the group was not found.</returns>
        Task<Group?> AddPartyToGroup(int userId, int groupId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a party from a group.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the group. Must be a valid user ID.</param>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="partyUuid">The UUID of the party to remove from the group.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated group, or null if the group or party was not found.</returns>
        Task<Group?> RemovePartyFromGroup(int userId, int groupId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a party to the favorites group for a given user
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result with a boolean telling whether the party was added as a favorite or if it already existed.</returns>
        Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a party from the favorites group for a given user
        /// </summary>
        Task<bool> DeleteFromFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
