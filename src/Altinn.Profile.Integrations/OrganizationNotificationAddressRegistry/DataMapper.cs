using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using static Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models.DigitalContactPointModel;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry
{
    /// <summary>
    /// Maps from the registry raw data to the data model stored in the database
    /// </summary>
    public static class DataMapper
    {
        /// <summary>
        /// A const for decalring the identifier type as organizationNumber
        /// </summary>
        public const string OrganizationNumberType = "ORGANISASJONSNUMMER";

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

        /// <summary>
        /// Maps from an organization notification address to the registry request
        /// </summary>
        public static RegistryRequest MapToRegistryRequest(NotificationAddress notificationAddress, string organizationNumber)
        {
            var request = new RegistryRequest
            {
                ContactInfo = new ContactInfoModel
                {
                    UnitContactInfo = new UnitContactInfoModel
                    {
                        UnitIdentifier = new UnitIdentifierModel
                        {
                            Value = organizationNumber,
                            Type = OrganizationNumberType,
                        },
                    },
                    DigitalContactPoint = MapDigitalContactPoint(notificationAddress),
                }
            };

            return request;
        }

        private static DigitalContactPointModel MapDigitalContactPoint(NotificationAddress notificationAddress)
        {
            if (notificationAddress.AddressType == AddressType.Email)
            {
                return new DigitalContactPointModel
                {
                    EmailAddress = new EmailAddressModel
                    {
                        Domain = notificationAddress.Domain,
                        Username = notificationAddress.Address,
                    },
                };
            }
            else
            {
                return new DigitalContactPointModel
                {
                    PhoneNumber = new PhoneNumberModel
                    {
                        Prefix = notificationAddress.Domain,
                        NationalNumber = notificationAddress.Address,
                        Name = notificationAddress.NotificationName,
                    },
                };
            }
        }

        /// <summary>
        /// Maps from the core data model to the data model stored in the database
        /// </summary>
        public static NotificationAddressDE MapFromCoreModelNotificationAddress(OrganizationDE organization, NotificationAddress notificationAddress, string registryId)
        {
            var organizationNotificationAddress = new NotificationAddressDE
            {
                RegistryOrganizationId = organization.RegistryOrganizationId,
                UpdateSource = UpdateSource.Altinn,
                HasRegistryAccepted = false,
                IsSoftDeleted = false,
                Address = notificationAddress.Address,
                Domain = notificationAddress.Domain,
                FullAddress = notificationAddress.FullAddress,
                AddressType = notificationAddress.AddressType,
                NotificationName = notificationAddress.NotificationName,
                RegistryID = registryId,
            };

            return organizationNotificationAddress;
        }
    }
}
