namespace Altinn.Profile.Core.User.PartyGroups
{
    /// <summary>
    /// Interface for the party group service
    /// </summary>
    public interface IPartyGroupService
    {
        /// <summary>
        /// Gets a group for a given user and group id
        /// </summary>
        /// <param name="userId">The logged in users userId</param>
        /// <param name="groupId">The group id of the group to retrieve</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        Task<Group?> GetGroup(int userId, int groupId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all groups for a given user. If none are found, an empty list is returned.
        /// </summary>
        /// <param name="userId">The identifier of the user whose groups are to be retrieved. Must be a valid user ID.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        Task<List<Group>> GetGroupsForAUser(int userId, CancellationToken cancellationToken);
        
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
        /// Gets the favorite parties for a given user. If no favorites are added, an empty group will be returned.
        /// </summary>
        Task<Group> GetFavorites(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Mark a party as a favorite for the current user
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result with a boolean telling whether the party was added as a favorite or if it already existed.</returns>
        Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Delete the given party from a users list of favorites.
        /// </summary>
        Task<bool> DeleteFromFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);
   }
}
