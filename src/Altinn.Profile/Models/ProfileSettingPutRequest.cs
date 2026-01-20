using System;
using System.ComponentModel.DataAnnotations;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a request to update profile settings.
    /// Inherits preferences from <see cref="ProfileSettingPreference"/>.
    /// </summary>
    public class ProfileSettingPutRequest : ProfileSettingPreference
    {
        /// <summary>
        /// Gets or sets the user's language preference in Altinn.
        /// </summary>
        [Required]
        public new string Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the users want
        /// to be asked for the party on every form submission.
        /// </summary>
        [Required]
        public new bool DoNotPromptForParty { get; set; }

        /// <summary>
        /// The UUID of the preselected party. Optional.
        /// </summary>
        [Required]
        public new Guid? PreselectedPartyUuid { get; set; }

        /// <summary>
        /// Indicates whether client units should be shown.
        /// </summary>
        [Required]
        public new bool ShowClientUnits { get; set; }

        /// <summary>
        /// Indicates whether sub-entities should be shown.
        /// </summary>
        [Required]
        public new bool ShouldShowSubEntities { get; set; }

        /// <summary>
        /// Indicates whether deleted entities should be shown.
        /// </summary>
        [Required]
        public new bool ShouldShowDeletedEntities { get; set; }
    }
}
