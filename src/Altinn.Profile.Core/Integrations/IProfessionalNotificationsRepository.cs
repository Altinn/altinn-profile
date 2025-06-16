using Altinn.Profile.Core.ProfessionalNotificationAddresses;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface for managing professional notification addresses.
    /// </summary>
    public interface IProfessionalNotificationsRepository
    {
        /// <summary>
        /// Retrieves the notification addresses for a given user and party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<UserPartyContactInfo?> GetNotificationAddresses(int userId, Guid partyUuid, CancellationToken cancellationToken);
    }
}
