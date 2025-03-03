#nullable disable

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// class for notifications addresses for organizations 
    /// </summary>
    [Table("notifications_address", Schema = "organization_notification_address")]
    public class OrganizationNotificationAddress
    {
        /// <summary>
        /// Gets or sets <see cref="NotificationAddressID"/>
        /// </summary>
        [Required]
        [Column("contact_info_id")]
        public int NotificationAddressID { get; set; }

        /// <summary>
        /// Gets or sets <see cref="RegistryOrganizationID"/>
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("registry_organization_id")]
        public string RegistryOrganizationID { get; set; }

        /// <summary>
        /// Gets or sets <see cref="RegistryID"/>
        /// </summary>
        [Required]
        [StringLength(32)]
        [Column("registry_id")]
        public string RegistryID { get; set; }

        /// <summary>
        /// Gets or sets AddressType, Email,Phone
        /// </summary>
        [Required]
        [Column("address_type")]
        public AddressType AddressType { get; set; }

        /// <summary>
        /// Gets or sets Domain part of the Address. In case phone the country code, in case of email the domain address
        /// </summary>
        [StringLength(200)]
        [Required]
        [Column("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets Address, email address if address type is email, phone number if type is SMS
        /// </summary>
        [StringLength(200)]
        [Column("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets FullAddress, either full email address or international country prefix and phone number
        /// </summary>
        [StringLength(200)]
        [Required]
        [Column("full_address")]
        public string FullAddress { get; set; }

        /// <summary>
        /// Gets or sets CreatedDateTime, the time when the items is initially saved in the local database
        /// </summary>
        [Required]
        [Column("created_date_time")]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets <see cref="RegistryUpdatedDateTime"/>, the time when the item is updated in the origin system
        /// </summary>
        [Column("registry_updated_date_time")]
        public DateTime? RegistryUpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the originator that did the last change
        /// </summary>
        [Required]
        [Column("update_source")]
        public UpdateSource UpdateSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the endpoint has been accepted and received from kofuvi registry 
        /// </summary>
        [Column("has_registry_accepted")]
        public bool? HasRegistryAccepted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is deleted in Altinn.
        /// </summary>
        [Column("is_soft_deleted")]
        public bool? IsSoftDeleted { get; set; }

        /// <summary>
        /// Gets or sets name of the contact point 
        /// </summary>
        [StringLength(200)]
        [Column("notification_name")]
        public string NotificationName { get; set; }
    }
}
