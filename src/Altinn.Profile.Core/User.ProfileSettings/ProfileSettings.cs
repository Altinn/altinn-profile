namespace Altinn.Profile.Core.User.ProfileSettings
{
    /// <summary>
    /// Represents user-specific portal settings and preferences.
    /// </summary>
    public class ProfileSettings
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The language the user has selected in Altinn portal.
        /// </summary>
        public required string LanguageType { get; set; }

        /// <summary>
        /// Indicates whether the user should not be prompted for party selection.
        /// Can be set without using PreselectedPartyUuid.
        /// </summary>
        public bool DoNotPromptForParty { get; set; }

        /// <summary>
        /// The UUID of the preselected party. Optional.
        /// </summary>
        public Guid? PreselectedPartyUuid { get; set; }

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

        /// <summary>
        /// The users last timestamp for ignoring the UnitProfile update
        /// </summary>
        public DateTime? IgnoreUnitProfileDateTime { get; set; }

        /// <summary>
        /// Sets the properties of this instance to the values from another instance.
        /// </summary>
        public void UpdateFrom(ProfileSettings other)
        {
            LanguageType = other.LanguageType;
            DoNotPromptForParty = other.DoNotPromptForParty;
            PreselectedPartyUuid = other.PreselectedPartyUuid;
            ShowClientUnits = other.ShowClientUnits;
            ShouldShowSubEntities = other.ShouldShowSubEntities;
            ShouldShowDeletedEntities = other.ShouldShowDeletedEntities;

            // Only update IgnoreUnitProfileDateTime if the other instance has a value
            if (other.IgnoreUnitProfileDateTime.HasValue)
            {
                IgnoreUnitProfileDateTime = other.IgnoreUnitProfileDateTime;
            }
        }

        /// <summary>
        /// Sets the properties of this instance to the values from a path request with optional values.
        /// </summary>
        public void UpdateFrom(ProfileSettingsPatchRequest other)
        {
            LanguageType = other.Language ?? LanguageType;
            DoNotPromptForParty = other.DoNotPromptForParty ?? DoNotPromptForParty;
            ShowClientUnits = other.ShowClientUnits ?? ShowClientUnits;
            ShouldShowSubEntities = other.ShouldShowSubEntities ?? ShouldShowSubEntities;
            ShouldShowDeletedEntities = other.ShouldShowDeletedEntities ?? ShouldShowDeletedEntities;

            // Only update PreselectedPartyUuid if the other instance has a value (null or Guid)
            if (other.PreselectedPartyUuid.HasValue)
            {
                PreselectedPartyUuid = other.PreselectedPartyUuid.Value;
            }
        }
    }
}
