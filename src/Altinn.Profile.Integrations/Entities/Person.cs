﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Entities;

/// <summary>
/// Represents a person in the contact and reservation schema.
/// </summary>
[Table("person", Schema = "contact_and_reservation")]
[Index("FnumberAk", Name = "person_fnumber_ak_key", IsUnique = true)]
public partial class Person
{
    /// <summary>
    /// Gets or sets the unique identifier for the contact and reservation user.
    /// </summary>
    [Key]
    [Column("contact_and_reservation_user_id")]
    public int ContactAndReservationUserId { get; set; }

    /// <summary>
    /// Gets or sets the F-number (a unique identifier) of the person.
    /// </summary>
    [Required]
    [Column("fnumber_ak")]
    [StringLength(11)]
    public string FnumberAk { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the person has a reservation.
    /// </summary>
    [Column("reservation")]
    public bool? Reservation { get; set; }

    /// <summary>
    /// Gets or sets the description of the person.
    /// </summary>
    [Column("description")]
    [StringLength(20)]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the mobile phone number of the person.
    /// </summary>
    [Column("mobile_phone_number")]
    [StringLength(20)]
    public string MobilePhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the mobile phone number was last updated.
    /// </summary>
    [Column("mobile_phone_number_last_updated")]
    public DateTime? MobilePhoneNumberLastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the mobile phone number was last verified.
    /// </summary>
    [Column("mobile_phone_number_last_verified")]
    public DateTime? MobilePhoneNumberLastVerified { get; set; }

    /// <summary>
    /// Gets or sets the email address of the person.
    /// </summary>
    [Column("email_address")]
    [StringLength(400)]
    public string EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the email address was last updated.
    /// </summary>
    [Column("email_address_last_updated")]
    public DateTime? EmailAddressLastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the email address was last verified.
    /// </summary>
    [Column("email_address_last_verified")]
    public DateTime? EmailAddressLastVerified { get; set; }

    /// <summary>
    /// Gets or sets the mailbox address of the person.
    /// </summary>
    [Column("mailbox_address")]
    [StringLength(50)]
    public string MailboxAddress { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the mailbox supplier.
    /// </summary>
    [Column("mailbox_supplier_id_fk")]
    public int? MailboxSupplierIdFk { get; set; }

    /// <summary>
    /// Gets or sets the X.509 certificate of the person.
    /// </summary>
    [Column("x509_certificate")]
    public string X509Certificate { get; set; }

    /// <summary>
    /// Gets or sets the language code of the person.
    /// </summary>
    [Column("language_code")]
    [StringLength(2)]
    public string LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the mailbox supplier.
    /// </summary>
    [ForeignKey("MailboxSupplierIdFk")]
    [InverseProperty("People")]
    public virtual MailboxSupplier MailboxSupplierIdFkNavigation { get; set; }
}
