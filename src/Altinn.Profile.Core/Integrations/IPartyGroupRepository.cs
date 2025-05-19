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
    }
}
