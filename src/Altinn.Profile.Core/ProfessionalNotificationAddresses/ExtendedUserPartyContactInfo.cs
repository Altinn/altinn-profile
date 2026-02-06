using Altinn.Profile.Core.AddressVerifications.Models;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Data model for the personal notification address for an organization
    /// </summary>
    public class ExtendedUserPartyContactInfo : UserPartyContactInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedUserPartyContactInfo"/> class.
        /// </summary>
        public ExtendedUserPartyContactInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedUserPartyContactInfo"/> class.
        /// </summary>
        /// <param name="userPartyContactInfo">The contact info to extend</param>
        public ExtendedUserPartyContactInfo(UserPartyContactInfo userPartyContactInfo) : base()
        {
            if (userPartyContactInfo != null)
            {
                UserPartyContactInfoId = userPartyContactInfo.UserPartyContactInfoId;
                UserId = userPartyContactInfo.UserId;
                PartyUuid = userPartyContactInfo.PartyUuid;
                EmailAddress = userPartyContactInfo.EmailAddress;
                PhoneNumber = userPartyContactInfo.PhoneNumber;
                LastChanged = userPartyContactInfo.LastChanged;
                UserPartyContactInfoResources = userPartyContactInfo.UserPartyContactInfoResources;
            }
        }

        /// <summary>
        /// A boolean indicating whether confirmation is needed for the contact info
        /// </summary>
        public bool NeedsConfirmation { get; internal set; }

        /// <summary>
        /// The verification status of the email address. Null if no email address is set, or if the email address has not been verified.
        /// </summary>
        public VerificationType EmailVerificationStatus { get; internal set; }

        /// <summary>
        /// The verification status of the phone number. Null if no phone number is set, or if the phone number has not been verified.
        /// </summary>
        public VerificationType SmsVerificationStatus { get; internal set; }
    }
}
