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
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<List<Group>> GetGroups(int userId, bool filterOnlyFavorite, CancellationToken cancellationToken);
    }
}
