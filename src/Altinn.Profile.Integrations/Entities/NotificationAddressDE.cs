#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// class for notifications addresses for organizations 
    /// </summary>
    [Table("notifications_address", Schema = "organization_notification_address")]
    public class NotificationAddressDE
    {
        /// <summary>
        /// Incremental Id
        /// </summary>
        [Required]
        public int NotificationAddressID { get; set; }

        /// <summary>
        /// Id from the registry
        /// </summary>
        [Required]
        [StringLength(32)]
        public string RegistryID { get; set; }

        /// <summary>
        /// The AddressType, either Email or Sms
        /// </summary>
        [Required]
        public AddressType AddressType { get; set; }

        /// <summary>
        /// The domain part of the Address. In case of phone numbers the country code, in case of email the domain address
        /// </summary>
        [StringLength(200)]
        public string Domain { get; set; }

        /// <summary>
        /// The address, email address if address type is email, phone number if type is SMS
        /// </summary>
        [StringLength(200)]
        [Required]
        public string Address { get; set; }

        /// <summary>
        /// FullAddress, either full email address or international country prefix and phone number
        /// </summary>
        [StringLength(200)]
        [Required]
        public string FullAddress { get; set; }

        /// <summary>
        /// The time when the items is initially saved in the local database
        /// </summary>
        [Required]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// The time when the item is updated in the origin system
        /// </summary>
        public DateTime? RegistryUpdatedDateTime { get; set; }

        /// <summary>
        /// The originator that did the last change
        /// </summary>
        [Required]
        public UpdateSource UpdateSource { get; set; }

        /// <summary>
        /// A value indicating whether the endpoint has been accepted and received from kofuvi registry 
        /// </summary>
        public bool? HasRegistryAccepted { get; set; }

        /// <summary>
        /// A value indicating whether the entity is deleted in Altinn.
        /// </summary>
        public bool? IsSoftDeleted { get; set; }

        /// <summary>
        /// Name of the contact point 
        /// </summary>
        [StringLength(200)]
        public string NotificationName { get; set; }

        /// <summary>
        /// The id of the organization in our database
        /// </summary>
        [Required]
        [Column("fk_registry_organization_id")]
        public required int RegistryOrganizationId { get; set; }

        /// <summary>
        /// The organization the notification address is connected to
        /// </summary>
        [ForeignKey("RegistryOrganizationId")]
        [InverseProperty("NotificationAddresses")]
        public virtual OrganizationDE Organization { get; set; }
    }
}
