using Altinn.Profile.Core.PartyGroups;

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
        /// Gets the groups for a given user.
        /// </summary>
        /// <param name="userId">The logged in users userId</param>
        /// <param name="filterOnlyFavorite">A flag to indicate that ionly the favorite group should be fetched</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A <see cref="Task{TResult}"/> with the groups as a result.</returns>
        Task<List<Group>> GetGroups(int userId, bool filterOnlyFavorite, CancellationToken cancellationToken);

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
