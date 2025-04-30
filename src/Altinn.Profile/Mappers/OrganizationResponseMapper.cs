using System.Linq;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;

namespace Altinn.Profile.Mappers
{
    /// <summary>
    /// Maps from an organization to an organization reponse
    /// </summary>
    public static class OrganizationResponseMapper
    {
        /// <summary>
        /// Maps from an organization to an organization reponse
        /// </summary>
        public static OrganizationResponse ToOrganizationResponse(Organization organization)
        {
            var result = new OrganizationResponse
            {
                OrganizationNumber = organization.OrganizationNumber,
                NotificationAddresses = [.. organization.NotificationAddresses.Where(n => n.IsSoftDeleted != true).Select(ToNotificationAddressResponse)]
            };

            return result;
        }

        /// <summary>
        /// Maps from a notification address to a notification address reponse
        /// </summary>
        public static NotificationAddressResponse ToNotificationAddressResponse(NotificationAddress notificationAddress)
        {
            var response = new NotificationAddressResponse
            {
                NotificationAddressID = notificationAddress.NotificationAddressID,
            };

            if (notificationAddress.AddressType == AddressType.Email)
            {
                response.Email = notificationAddress.FullAddress;
            }
            else
            {
                response.Phone = notificationAddress.Address;
                response.CountryCode = notificationAddress.Domain;
            }

            return response;
        }
    }
}
