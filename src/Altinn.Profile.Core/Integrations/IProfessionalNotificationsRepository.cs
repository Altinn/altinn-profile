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
        Task<UserPartyContactInfo?> GetNotificationAddresses(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
