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
        public Task<UserPartyContactInfo?> GetNotificationAddresses(int userId, Guid partyUuid, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.GetNotificationAddress(userId, partyUuid, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> AddOrUpdateNotificationAddressesAsync(UserPartyContactInfo contactInfo, CancellationToken cancellationToken)
        {
            return _professionalNotificationsRepository.AddOrUpdateNotificationAddressAsync(contactInfo, cancellationToken);
        }
    }
}
