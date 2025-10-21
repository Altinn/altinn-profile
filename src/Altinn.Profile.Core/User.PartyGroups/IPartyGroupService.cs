namespace Altinn.Profile.Core.PartyGroups
{
    /// <summary>
    /// Interface for the party group service
    /// </summary>
    public interface IPartyGroupService
    {
        /// <summary>
        /// Retrieves all groups for a given user. If none are found, an empty list is returned.
        /// </summary>
        Task<List<Group>> GetGroupsForAUser(int userId, CancellationToken cancellationToken);

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
