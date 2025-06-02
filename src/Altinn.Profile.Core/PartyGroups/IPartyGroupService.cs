namespace Altinn.Profile.Core.PartyGroups
{
    /// <summary>
    /// Interface for the party group service
    /// </summary>
    public interface IPartyGroupService
    {
        /// <summary>
        /// Gets the favorite parties for a given user. If no favorites are added, an empty group will be returned.
        /// </summary>
        Task<Group> GetFavorites(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Mark a party as a favorite for the current user
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result with a boolean telling whether the party was added as a favorite or if it already existed.</returns>
        Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
