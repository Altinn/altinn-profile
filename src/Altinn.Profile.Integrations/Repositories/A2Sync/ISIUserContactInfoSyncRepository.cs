using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Repository for syncing self-identified user contact information from Altinn 2
    /// </summary>
    public interface ISIUserContactInfoSyncRepository
    {
        /// <summary>
        /// Inserts or updates the contact info for a self identified user. If the user already exists, the existing record will be updated with the new contact info. If the user does not exist, a new record will be created.
        /// </summary>
        /// <param name="userContactSettings">The contact info to insert or update</param>
        /// <param name="updatedDatetime">The datetime for when the contact info was updated in Altinn 2.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The inserted or updated user contact info</returns>
        Task<UserContactInfo> InsertOrUpdate(SiUserContactSettings userContactSettings, DateTime updatedDatetime, CancellationToken cancellationToken);
    }
}
