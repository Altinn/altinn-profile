namespace Altinn.Profile.Core.PartyGroups
{
    /// <summary>
    /// Interface for the party group service
    /// </summary>
    public interface IPartyGroupService
    {
        /// <summary>
        /// Gets the favorite parties for a given user
        /// </summary>
        Task<int[]> GetFavorites(int userId, CancellationToken cancellationToken);
    }
}
