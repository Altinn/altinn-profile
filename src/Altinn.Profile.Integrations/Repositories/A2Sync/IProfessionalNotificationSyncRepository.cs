using Altinn.Profile.Core.ProfessionalNotificationAddresses;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Represents an implementation contract for a repository that can handle professional notification addresses.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public interface IProfessionalNotificationSyncRepository
    {
        /// <summary>
        /// Adds a new or updates an existing notification address for a user and party.
        /// </summary>
        /// <param name="contactInfo">The contact info to be added</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> containing a boolean value indicating if the value was added or not.         
        /// </returns>
        Task AddOrUpdateNotificationAddressFromSyncAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the notification addresses that the given user has associated with the given party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing the identified notification addresses or null if there are none.</returns>
        Task<UserPartyContactInfo?> DeleteNotificationAddressFromSyncAsync(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
