using System.Linq;
using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Extension class to map from Notification address input model 
    /// </summary>
    public static class NotificationAddressRequestMapper
    {
        /// <summary>
        /// Maps from notification address input model to notification address core model
        /// </summary>
        public static NotificationAddress ToInternalModel(NotificationAddressModel notificationAddress)
        {
            NotificationAddress coreModel;

            // The notificationAddressModel validates that either Phone or Email must be specified
            if (!string.IsNullOrEmpty(notificationAddress.Email))
            {
                var emailParts = notificationAddress.Email.Trim().Split('@');
                coreModel = new NotificationAddress
                {
                    AddressType = AddressType.Email,
                    Address = emailParts[0],
                    Domain = emailParts[^1],
                    FullAddress = notificationAddress.Email.Trim()
                };
            }
            else
            {
                coreModel = new NotificationAddress
                {
                    AddressType = AddressType.SMS,
                    Address = notificationAddress.Phone.Trim(),
                    Domain = notificationAddress.CountryCode?.Trim(),
                    FullAddress = notificationAddress.CountryCode?.Trim() + notificationAddress.Phone.Trim()
                };
            }

            return coreModel;
        }
    }
}
