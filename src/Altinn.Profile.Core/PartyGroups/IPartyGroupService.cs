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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task MarkPartyAsFavorite(int userId, int partyId, CancellationToken cancellationToken);
    }
}
