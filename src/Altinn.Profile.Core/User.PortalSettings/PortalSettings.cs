namespace Altinn.Profile.Core.User.PortalSettings
{
    /// <summary>
    /// Represents user-specific portal settings and preferences.
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
        public LanguageType LanguageType { get; set; }

        /// <summary>
        /// Indicates whether the user should not be prompted for party selection.
        /// </summary>
        public bool DoNotPromptForParty { get; set; }

        /// <summary>
        /// The UUID of the preselected party.
        /// </summary>
        public Guid PreselectedPartyUuid { get; set; }

        /// <summary>
        /// Indicates whether client units should be shown.
        /// </summary>
        public bool ShowClientUnits { get; set; }

        /// <summary>
        /// Indicates whether sub-entities should be shown.
        /// </summary>
        public bool ShouldShowSubEntities { get; set; }

        /// <summary>
        /// Indicates whether deleted entities should be shown.
        /// </summary>
        public bool ShouldShowDeletedEntities { get; set; }
    }
}
