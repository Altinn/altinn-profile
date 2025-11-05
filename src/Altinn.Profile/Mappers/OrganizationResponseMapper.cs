using System.Linq;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;

namespace Altinn.Profile.Mappers
{
    /// <summary>
    /// Maps from an organization to an organization response
    /// </summary>
    public static class OrganizationResponseMapper
    {
        /// <summary>
        /// Maps from an organization to an organization response
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
        /// Maps from a notification address to a notification address response
        /// </summary>
        public static NotificationAddressResponse ToNotificationAddressResponse(NotificationAddress notificationAddress)
        {
            var response = new NotificationAddressResponse
            {
                NotificationAddressId = notificationAddress.NotificationAddressID,
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

        /// <summary>
        /// Maps from a Dashboard notification address to a Dashboard notification address response
        /// </summary>
        public static DashboardNotificationAddressResponse ToDashboardNotificationAddressResponse(
            NotificationAddress notificationAddress,
            string requestedOrgNumber,
            string sourceOrgNumber)
        {
            var response = new DashboardNotificationAddressResponse
            {
                NotificationAddressId = notificationAddress.NotificationAddressID,
                RequestedOrgNumber = requestedOrgNumber,
                SourceOrgNumber = sourceOrgNumber,
                LastChangedTimeStamp = notificationAddress.RegistryUpdatedDateTime
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
