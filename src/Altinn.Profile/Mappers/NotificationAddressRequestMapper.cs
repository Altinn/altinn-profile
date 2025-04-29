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
            var coreModel = new NotificationAddress();

            if (!string.IsNullOrEmpty(notificationAddress.Email))
            {
                var emailParts = notificationAddress.Email.Trim().Split('@');
                coreModel.Address = emailParts.First();
                coreModel.Domain = emailParts.Last();
                coreModel.AddressType = AddressType.Email;
                coreModel.FullAddress = notificationAddress.Email.Trim();
            }
            else if (!string.IsNullOrEmpty(notificationAddress.Phone))
            {
                coreModel.Address = notificationAddress.Phone.Trim();
                coreModel.Domain = notificationAddress.CountryCode?.Trim();
                coreModel.AddressType = AddressType.SMS;
                coreModel.FullAddress = coreModel.Domain + coreModel.Address;
            }

            return coreModel;
        }
    }
}
