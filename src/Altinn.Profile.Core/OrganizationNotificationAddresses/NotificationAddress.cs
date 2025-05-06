#nullable disable

namespace Altinn.Profile.Core.OrganizationNotificationAddresses
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class NotificationAddress
    {
        /// <summary>
        /// <see cref="NotificationAddressID"/>
        /// </summary>
        public int NotificationAddressID { get; init; }

        /// <summary>
        /// The AddressType, either Email or Sms
        /// </summary>
        public AddressType AddressType { get; init; }

        /// <summary>
        /// The domain part of the Address. In case of phone numbers the country code, in case of email the domain address
        /// </summary>
        public string Domain { get; init; }

        /// <summary>
        /// The address, email address if address type is email, phone number if type is SMS
        /// </summary>
        public string Address { get; init; }

        /// <summary>
        /// FullAddress, either full email address or international country prefix and phone number
        /// </summary>
        public string FullAddress { get; init; }

        /// <summary>
        /// Name of the contact point 
        /// </summary>
        public string NotificationName { get; init; }

        /// <summary>
        /// Id from the registry
        /// </summary>
        public string RegistryID { get; init; }

        /// <summary>
        /// A value indicating whether the entity is deleted in Altinn.
        /// </summary>
        public bool? IsSoftDeleted { get; init; }

        /// <summary>
        /// A value indicating whether the endpoint has been accepted and received from kofuvi registry 
        /// </summary>
        public bool? HasRegistryAccepted { get; set; }
    }
}
