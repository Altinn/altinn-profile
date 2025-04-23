using System.Linq;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;

namespace Altinn.Profile.Mappers
{
    /// <summary>
    /// Maps from an organization notificationAddress input model to the internal models
    /// </summary>
    public static class OrganizationRequestMapper
    {
        /// <summary>
        /// Maps from notification address request model to notification address core model
        /// </summary>
        public static NotificationAddress MapNotificationAddress(NotificationAddressModel notificationAddress)
        {
            var response = new NotificationAddress
            {
                NotificationAddressID = notificationAddress.NotificationAddressID,
                ToBeDeleted = notificationAddress.IsDeleted,
            };

            if (!string.IsNullOrEmpty(notificationAddress.Email))
            {
                response.Address = notificationAddress.Email.Trim().Split('@').First();
                response.Domain = notificationAddress.Email.Trim().Split('@').Last();
                response.AddressType = AddressType.Email;
                response.FullAddress = notificationAddress.Email.Trim();
            }
            else if (!string.IsNullOrEmpty(notificationAddress.Phone))
            {
                response.Address = notificationAddress.Phone.Trim();
                response.Domain = notificationAddress.CountryCode.Trim();
                response.AddressType = AddressType.SMS;
                response.FullAddress = notificationAddress.CountryCode + notificationAddress.Phone;
            }

            return response;
        }
    }
}
