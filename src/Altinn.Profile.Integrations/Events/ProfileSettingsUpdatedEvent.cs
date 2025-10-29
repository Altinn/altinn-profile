namespace Altinn.Profile.Integrations.Events
{
    /// <summary>
    /// Event representing an update to profile settings.
    /// </summary>
    /// <param name="UserId">The user ID associated with the update.</param>
    /// <param name="EventTimestamp">The timestamp when the event occurred.</param>
    /// <param name="LanguageType">The to character language type</param>
    /// <param name="DoNotPromptForParty">A value indicating whether the user should not be prompted for party selection.</param>
    /// <param name="PreselectedPartyUuid">The UUID of the preselected party. Optional.</param>
    /// <param name="ShowClientUnits">A value indicating whether client units should be shown.</param>
    /// <param name="ShouldShowSubEntities">A value indicating whether sub-entities should be shown.</param>
    /// <param name="ShouldShowDeletedEntities">A value indicating whether deleted entities should be shown.</param>
    /// <param name="IgnoreUnitProfileDateTime">The users last timestamp for ignoring the UnitProfile update.</param>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public record ProfileSettingsUpdatedEvent(
        int UserId,
        DateTime EventTimestamp,
        string LanguageType,
        bool DoNotPromptForParty,
        Guid? PreselectedPartyUuid,
        bool ShowClientUnits,
        bool ShouldShowSubEntities,
        bool ShouldShowDeletedEntities,
        DateTime? IgnoreUnitProfileDateTime);
}
