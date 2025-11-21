namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Represents an implementation contract for a business service that can handle professional notification addresses.
    /// </summary>
    public interface IProfessionalNotificationsService
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
        /// Retrieves all notification addresses for all parties for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing the list of identified notification addresses or an empty list if there are none.</returns>
        Task<IReadOnlyList<UserPartyContactInfo>> GetAllNotificationAddressesAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new or updates an existing notification address for a user and party.
        /// Returns <c>true</c> if a new record was created, <c>false</c> if an existing record was updated.
        /// </summary>
        /// <param name="contactInfo">The contact info to be added</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> containing a boolean value indicating if the value was added or not.         
        /// Returns <c>true</c> if a new record was added, <c>false</c> if an existing record was updated.
        /// </returns>
        Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the notification addresses that the given user has associated with the given party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task with the return value containing the identified notification addresses or null if there are none.</returns>
        Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all user contact information registered for a specific organization, enriched with user identity (SSN and Name).
        /// Used by the Support Dashboard to display personal contact details that users have registered for acting on behalf of organizations.
        /// </summary>
        /// <param name="organizationNumber">The organization number to retrieve contact information for</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>List of user contact information with identity, or null if organization not found</returns>
        Task<List<UserPartyContactInfoWithIdentity>?> GetContactInformationByOrganizationNumberAsync(string organizationNumber, CancellationToken cancellationToken);
    }
}
