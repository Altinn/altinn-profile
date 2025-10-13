using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Initializes a new instance of the <see cref="ChangeLogItem"/> class.
/// </summary>
public class ChangeLogItem
{
    /// <summary>
    /// Gets or sets the unique index id created by the database when the entry was initially stored.
    /// </summary>
    /// <remarks>Should be empty when making new log entries.</remarks>
    public int ProfileChangeLogId { get; set; }

    /// <summary>
    /// Gets or sets the date and time for when the log entry was made.
    /// </summary>
    /// <remarks>Assigned by data access code during insert.</remarks>
    public DateTime LoggedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time for the last change (current).
    /// </summary>
    /// <remarks>Usually current time, but the plan is to populate the log with existing profiles.</remarks>
    public DateTime ChangeDatetime { get; set; }

    /// <summary>
    /// Gets or sets the date and time for when the changed object had its last change.
    /// </summary>
    /// <remarks>Assigned only if available and necessary for synchronization validation.</remarks>
    public DateTime? PreviousChangeDatetime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the type of change. e.g. insert, update or delete.
    /// </summary>
    public OperationType OperationType { get; set; }

    /// <summary>
    /// Gets or sets the type of object that was changed
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// Gets or sets the serialized representation of the new state of the changed object.
    /// </summary>
    public required string DataObject { get; set; }

    /// <summary>
    /// Gets or sets the Altinn version number for where the change originated. 2 or 3.
    /// </summary>
    public int ChangeSource { get; set; }

    /// <summary>
    /// Represents the updated data of a favorite for logging purposes.
    /// </summary>
    public class Favorite
    {
        /// <summary>
        /// Gets or sets the id of the user that made a change to favorites.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the uuid of the party that the user either added or removed from their favorites.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// This method will deserialize a JSON representation of the <see cref="Favorite"/> object.
        /// </summary>
        /// <returns>JSON deserialized version of the current object.</returns>
        public static Favorite? Deserialize(string data)
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<Favorite>(data, settings);
        }
    }

    /// <summary>
    /// Represents the updated data of a notification setting for logging purposes.
    /// </summary>
    public class ProfessionalNotificationSettings
    {
        /// <summary>
        /// Gets or sets the id of the user that made a change to professional notification settings.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the uuid of the party that the user either added or removed from their professional notification settings.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the phone number to use for SMS notifications.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address to use for email notifications.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the service options for which the reportee wants to receive notifications.
        /// </summary>  
        public string[] ServiceOptions { get; set; } = [];

        /// <summary>
        /// This method will deserialize the JSON representation of the <see cref="ProfessionalNotificationSettings"/> object.
        /// </summary>
        /// <returns>The object deserialized from JSON.</returns>
        public static ProfessionalNotificationSettings? Deserialize(string data)
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<ProfessionalNotificationSettings>(data, settings);
        }
    }

    /// <summary>
    /// Represents the updated data of a portal setting.
    /// </summary>
    public class PortalSettings
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The language the user has selected in Altinn portal.
        /// </summary>
        public int LanguageType { get; set; }

        /// <summary>
        /// Indicates whether the user should not be prompted for party selection.
        /// CCan be set without using PreselectedPartyUuid.
        /// </summary>
        [JsonConverter(typeof(IntToBoolConverter))]
        public bool DoNotPromptForParty { get; set; }

        /// <summary>
        /// The UUID of the preselected party.Optional.
        /// </summary>
        [JsonConverter(typeof(StringToNullableGuidConverter))]
        public Guid? PreselectedPartyUuid { get; set; }

        /// <summary>
        /// Indicates whether client units should be shown.
        /// </summary>
        [JsonConverter(typeof(IntToBoolConverter))]
        public bool ShowClientUnits { get; set; }

        /// <summary>
        /// Indicates whether sub-entities should be shown.
        /// </summary>
        [JsonConverter(typeof(IntToBoolConverter))]
        public bool ShouldShowSubEntities { get; set; }

        /// <summary>
        /// Indicates whether deleted entities should be shown.
        /// </summary>
        [JsonConverter(typeof(IntToBoolConverter))]
        public bool ShouldShowDeletedEntities { get; set; }

        /// <summary>
        /// The users last timestamp for ignoring the UnitProfile update
        /// </summary>
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? IgnoreUnitProfileDateTime { get; set; }

        /// <summary>
        /// This method will deserialize the JSON representation of the <see cref="PortalSettings"/> object.
        /// </summary>
        /// <returns>The object deserialized from JSON.</returns>
        public static PortalSettings? Deserialize(string data)
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<PortalSettings>(data, settings);
        }
    }
}
