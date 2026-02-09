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
        /// <param name="needsConfirmation">A flag to indicate wether the address needs to be confirmed by the user</param>
        /// <param name="emailVerificationType">The verification status of the email address</param>
        /// <param name="smsVerificationType">The verification status of the phone number for sms.</param>
        public ExtendedUserPartyContactInfo(UserPartyContactInfo userPartyContactInfo, bool needsConfirmation, VerificationType? emailVerificationType, VerificationType? smsVerificationType) : base()
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

            NeedsConfirmation = needsConfirmation;
            EmailVerificationStatus = emailVerificationType;
            SmsVerificationStatus = smsVerificationType;
        }

        /// <summary>
        /// A boolean indicating whether confirmation is needed for the contact info
        /// </summary>
        public bool NeedsConfirmation { get; init; }

        /// <summary>
        /// The verification status of the email address. Null if no email address is set.
        /// </summary>
        public VerificationType? EmailVerificationStatus { get; init; }

        /// <summary>
        /// The verification status of the phone number. Null if no phone number is set.
        /// </summary>
        public VerificationType? SmsVerificationStatus { get; init; }
    }
}
