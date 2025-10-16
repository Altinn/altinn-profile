namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Interface to delete or insert favorites to the DB without notifying A2
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public interface IFavoriteSyncRepository
    {
        /// <summary>
        /// Adds a party to the favorites group for a given user
        /// </summary>
        Task AddPartyToFavorites(int userId, Guid partyUuid, DateTime created, CancellationToken cancellationToken);

        /// <summary>
        /// Removes a party from the favorites group for a given user
        /// </summary>
        Task DeleteFromFavorites(int userId, Guid partyUuid, DateTime deleted, CancellationToken cancellationToken);
    }
}
