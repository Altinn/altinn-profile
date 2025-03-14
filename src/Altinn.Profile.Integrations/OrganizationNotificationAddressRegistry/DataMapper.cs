using Altinn.Profile.Integrations.Entities;
using static Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.EntryContent.ContactPointModel.DigitalContactPointModel;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry
{
    /// <summary>
    /// Maps from the registry raw data to the data model stored in the database
    /// </summary>
    public static class DataMapper
    {
        /// <summary>
        /// Maps from the registry raw data to the data model stored in the database
        /// </summary>
        public static OrganizationNotificationAddress MapOrganizationNotificationAddress(Entry entry, Organization organization)
        {
            var organizationNotificationAddress = new OrganizationNotificationAddress
            {
                RegistryOrganizationId = organization.RegistryOrganizationId,
                RegistryID = entry.Id,
                RegistryUpdatedDateTime = entry.Updated,
                UpdateSource = UpdateSource.KoFuVi,
                HasRegistryAccepted = true,
                IsSoftDeleted = false,
                NotificationName = entry.Title,
            };

            var contanctPoint = entry.Content.ContactPoint.DigitalContactPoint;
            var isEmail = contanctPoint.EmailAddress != null;

            if (isEmail)
            {
                MapEmailSpecificValues(organizationNotificationAddress, contanctPoint.EmailAddress);
            }
            else
            {
                MapPhoneSpecificDetails(organizationNotificationAddress, contanctPoint.PhoneNumber);
            }

            return organizationNotificationAddress;
        }

        private static void MapPhoneSpecificDetails(OrganizationNotificationAddress organizationNotificationAddress, PhoneNumberModel phoneNumber)
        {
            string? rawDataPrefix = string.IsNullOrEmpty(phoneNumber.Prefix) ? null : phoneNumber.Prefix.Trim();
            string? prefix;
            if (string.IsNullOrEmpty(rawDataPrefix) || rawDataPrefix.StartsWith('+'))
            {
                prefix = rawDataPrefix;
            }
            else if (rawDataPrefix.StartsWith("00"))
            {
                prefix = '+' + rawDataPrefix.TrimStart('0');
            }
            else
            {
                prefix = '+' + rawDataPrefix;
            }

            organizationNotificationAddress.AddressType = AddressType.SMS;
            organizationNotificationAddress.Domain = prefix;
            organizationNotificationAddress.Address = phoneNumber.NationalNumber;
            organizationNotificationAddress.FullAddress = string.Concat(prefix, phoneNumber.NationalNumber);
        }

        private static void MapEmailSpecificValues(OrganizationNotificationAddress organizationNotificationAddress, EmailAddressModel emailAddress)
            {
            organizationNotificationAddress.AddressType = AddressType.Email;
            organizationNotificationAddress.Domain = emailAddress.Domain;
            organizationNotificationAddress.Address = emailAddress.Username;
            organizationNotificationAddress.FullAddress = string.Concat(emailAddress.Username, '@', emailAddress.Domain);
        }
    }
}
