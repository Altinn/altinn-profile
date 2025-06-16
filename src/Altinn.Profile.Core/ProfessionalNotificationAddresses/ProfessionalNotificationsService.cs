using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Service for handling professional notification addresses.
    /// </summary>
    public class ProfessionalNotificationsService(IProfessionalNotificationsRepository professionalNotificationsRepository) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;

        /// <summary>
        /// Retrieves notification addresses for a given user and party.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="partyUuid">The UUID of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task<UserPartyContactInfo?> GetNotificationAddresses(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.GetNotificationAddresses(userId, partyUuid, cancellationToken);
        }
    }
}
