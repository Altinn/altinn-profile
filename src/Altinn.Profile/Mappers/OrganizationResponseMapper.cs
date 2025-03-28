using System.Collections.Generic;
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
        public static OrganizationResponse MapResponse(Organization organization)
        {
            var result = new OrganizationResponse
            {
                OrganizationNumber = organization.OrganizationNumber,
                NotificationAddresses = organization.NotificationAddresses.Where(n => n.IsSoftDeleted != true).Select(MapNotificationAddress).ToList()
            };

            return result;
        }

        private static OrganizationResponse.NotificationAddress MapNotificationAddress(NotificationAddress notificationAddress)
        {
            var response = new OrganizationResponse.NotificationAddress
            {
                RegistryID = notificationAddress.RegistryID,
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
