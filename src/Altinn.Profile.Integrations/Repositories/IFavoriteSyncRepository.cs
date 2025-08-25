namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Interface to delete or insert favorites to the DB without notifying A2
    /// </summary>
    public interface IFavoriteSyncRepository
    {
        /// <summary>
        /// Adds a party to the favorites group for a given user
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result with a boolean telling whether the party was added as a favorite or if it already existed.</returns>
        Task<bool> AddPartyToFavorites(int userId, Guid partyUuid, DateTime created, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a party from the favorites group for a given user
        /// </summary>
        Task<bool> DeleteFromFavorites(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
