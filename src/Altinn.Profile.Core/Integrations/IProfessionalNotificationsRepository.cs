using Altinn.Profile.Core.ProfessionalNotificationAddresses;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Represents an implementation contract for a repository that can handle professional notification addresses.
    /// </summary>
    public interface IProfessionalNotificationsRepository
    {
        /// <summary>
        /// Retrieves the notification addresses that the given user has associated with the given party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing the identified notification addresses or null if there are none.</returns>
        Task<UserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all notification addresses for all parties for the given user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing a list of notification addresses.</returns>
        Task<IReadOnlyList<UserPartyContactInfo>> GetAllNotificationAddressesForUserAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new or updates an existing notification address for a user and party.
        /// Returns <c>true</c> if a new record was created, <c>false</c> if an existing record was updated.
        /// </summary>
        /// <param name="contactInfo">The contact info to be added</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> containing a boolean value indicating if the value was added or not.         
        /// Returns <c>true</c> if a new record was added, <c>false</c> if an existing record was updated.
        /// </returns>
        Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the notification addresses that the given user has associated with the given party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing the identified notification addresses or null if there are none.</returns>
        Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
