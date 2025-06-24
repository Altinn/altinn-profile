using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Service for handling professional notification addresses.
    /// </summary>
    public class ProfessionalNotificationsService(IProfessionalNotificationsRepository professionalNotificationsRepository) : IProfessionalNotificationsService
    {
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository = professionalNotificationsRepository;

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> GetNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.GetNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> AddOrUpdateNotificationAddressAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.AddOrUpdateNotificationAddressAsync(contactInfo, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<UserPartyContactInfo?> DeleteNotificationAddressAsync(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.DeleteNotificationAddressAsync(userId, partyUuid, cancellationToken);
        }
    }
}
