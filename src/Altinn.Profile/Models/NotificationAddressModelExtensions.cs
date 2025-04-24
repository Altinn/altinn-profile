using System.Linq;
using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Extension class to map from Notification address input model 
    /// </summary>
    public static class NotificationAddressModelExtensions
    {
        /// <summary>
        /// Maps from notification address input model to notification address core model
        /// </summary>
        public static NotificationAddress ToInternalModel(this NotificationAddressModel notificationAddress)
        {
            var response = new NotificationAddress
            {
                ToBeDeleted = notificationAddress.IsDeleted,
            };

            if (!string.IsNullOrEmpty(notificationAddress.Email))
            {
                var emailParts = notificationAddress.Email.Trim().Split('@');
                response.Address = emailParts.First();
                response.Domain = emailParts.Last();
                response.AddressType = AddressType.Email;
                response.FullAddress = notificationAddress.Email.Trim();
            }
            else if (!string.IsNullOrEmpty(notificationAddress.Phone))
            {
                response.Address = notificationAddress.Phone.Trim();
                response.Domain = notificationAddress.CountryCode.Trim();
                response.AddressType = AddressType.SMS;
                response.FullAddress = response.Domain + response.Address;
            }

            return response;
        }
    }
}
