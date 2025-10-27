namespace Altinn.Profile.Integrations.SblBridge.User.ProfileSettings
{
    /// <summary>
    /// Describes an event where a user made some change to their portal settings preferences.
    /// </summary>
    public class ProfileSettingsChangedRequest
    {
        /// <summary>
        /// Gets or sets the type of change. Supported values are "insert" and "delete".
        /// </summary>
        public required string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the date and time for the change.
        /// </summary>
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the language the user has selected in Altinn portal.
        /// </summary>
        public required string LanguageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user should not be prompted for party selection.
        /// Can be set without using PreselectedPartyUuid.
        /// </summary>
        public bool DoNotPromptForParty { get; set; }

        /// <summary>
        /// Gets or sets the UUID of the preselected party. Optional.
        /// </summary>
        public Guid? PreselectedPartyUuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether client units should be shown.
        /// </summary>
        public bool ShowClientUnits { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sub-entities should be shown.
        /// </summary>
        public bool ShouldShowSubEntities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deleted entities should be shown.
        /// </summary>
        public bool ShouldShowDeletedEntities { get; set; }

        /// <summary>
        /// Gets or sets the users last timestamp for ignoring the UnitProfile update
        /// </summary>
        public DateTime? IgnoreUnitProfileDateTime { get; set; }
    }
}
