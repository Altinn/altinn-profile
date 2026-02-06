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
        /// <param name="generateVerificationCode">A feature flag to indicate whether a verification code should be generated and sent to the provided email address or phone number. This is used to verify that the user has access to the provided contact information before it is saved as a notification address. If set to true, a verification code will be generated and sent, and the user will need to verify the code before the notification address is considered valid.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> containing a boolean value indicating if the value was added or not.         
        /// Returns <c>true</c> if a new record was added, <c>false</c> if an existing record was updated.
        /// </returns>
        Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, bool generateVerificationCode, CancellationToken cancellationToken);

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
        /// <returns>Read-only list of user contact information with identity, or null if organization not found</returns>
        Task<IReadOnlyList<UserPartyContactInfoWithIdentity>?> GetContactInformationByOrganizationNumberAsync(string organizationNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all user contact information registered for a specific organization, enriched with user identity (SSN and Name).
        /// Used by the Support Dashboard to display personal contact details that users have registered.
        /// </summary>
        /// <param name="emailAddress">The email address to retrieve contact information for</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Read-only list of user contact information with identity, or an empty list if none found</returns>
        Task<IReadOnlyList<UserPartyContactInfoWithIdentity>> GetContactInformationByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all user contact information registered for a specific organization, enriched with user identity (SSN and Name).
        /// Used by the Support Dashboard to display personal contact details that users have registered.
        /// </summary>
        /// <param name="phoneNumber">The phone number to retrieve contact information for</param>
        /// <param name="countryCode">The country code of the phone number</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Read-only list of user contact information with identity, or an empty list if none found</returns>
        Task<IReadOnlyList<UserPartyContactInfoWithIdentity>> GetContactInformationByPhoneNumberAsync(string phoneNumber, string countryCode, CancellationToken cancellationToken);
    }
}
