namespace Altinn.Profile.Integrations.Authorization
{
    /// <summary>
    /// Interface for authorization functionality.
    /// </summary>
    public interface IAuthorizationClient
    {
        /// <summary>
        /// Verifies that the selected party is contained in the user's party list.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="partyId">The party id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> Boolean indicating whether or not the user can represent the selected party.</returns>
        Task<bool> ValidateSelectedParty(int userId, int partyId, CancellationToken cancellationToken);
    }
}
