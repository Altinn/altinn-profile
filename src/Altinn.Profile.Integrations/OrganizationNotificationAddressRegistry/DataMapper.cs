﻿using Altinn.Profile.Core.OrganizationNotificationAddresses;
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
        public static NotificationAddressDE MapOrganizationNotificationAddress(Entry entry, OrganizationDE organization)
        {
            var organizationNotificationAddress = new NotificationAddressDE
            {
                RegistryOrganizationId = organization.RegistryOrganizationId,
                RegistryID = entry.Id,
                RegistryUpdatedDateTime = entry.Updated,
                UpdateSource = UpdateSource.KoFuVi,
                HasRegistryAccepted = true,
                IsSoftDeleted = false,
                NotificationName = entry.Title,
            };

            var contanctPoint = entry.Content?.ContactPoint?.DigitalContactPoint;

            if (contanctPoint?.EmailAddress != null)
            {
                MapEmailSpecificValues(organizationNotificationAddress, contanctPoint.EmailAddress);
            }
            else if (contanctPoint?.PhoneNumber != null)
            {
                MapPhoneSpecificDetails(organizationNotificationAddress, contanctPoint.PhoneNumber);
            }
            else
            {
                throw new OrganizationNotificationAddressChangesException(string.Concat("NotificationAddress type not recognized for feed entry with id: ", entry.Id));
            }

            return organizationNotificationAddress;
        }

        private static void MapPhoneSpecificDetails(NotificationAddressDE organizationNotificationAddress, PhoneNumberModel phoneNumber)
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

        private static void MapEmailSpecificValues(NotificationAddressDE organizationNotificationAddress, EmailAddressModel emailAddress)
            {
            organizationNotificationAddress.AddressType = AddressType.Email;
            organizationNotificationAddress.Domain = emailAddress.Domain;
            organizationNotificationAddress.Address = emailAddress.Username;
            organizationNotificationAddress.FullAddress = string.Concat(emailAddress.Username, '@', emailAddress.Domain);
        }
    }
}
