#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
        public int NotificationAddressID { get; set; }

        /// <summary>
        /// The AddressType, either Email or Sms
        /// </summary>
        public AddressType AddressType { get; set; }

        /// <summary>
        /// The domain part of the Address. In case of phone numbers the country code, in case of email the domain address
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The address, email address if address type is email, phone number if type is SMS
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// FullAddress, either full email address or international country prefix and phone number
        /// </summary>
        public string FullAddress { get; set; }

        /// <summary>
        /// Name of the contact point 
        /// </summary>
        public string NotificationName { get; set; }

        /// <summary>
        /// Id from the registry
        /// </summary>
        public string RegistryID { get; set; }

        /// <summary>
        /// A value indicating whether the entity is deleted in Altinn.
        /// </summary>
        public bool? IsSoftDeleted { get; set; }

        /// <summary>
        /// A value indicating whether the entity should be deleted in Altinn.
        /// </summary>
        public bool? ToBeDeleted { get; set; }

        /// <summary>
        /// Gets or sets status message of operation against the registry
        /// </summary>
        [DataMember]
        public string UpdateMessage { get; set; }
    }
}
