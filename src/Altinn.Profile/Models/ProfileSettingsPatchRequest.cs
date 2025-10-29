#nullable enable
using System;

using Altinn.Profile.Core.Utils;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents user-specific portal settings and preferences.
    /// </summary>
    public class ProfileSettingsPatchRequest
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The language the user has selected in Altinn portal.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Indicates whether the user should not be prompted for party selection.
        /// Can be set without using PreselectedPartyUuid.
        /// </summary>
        public bool? DoNotPromptForParty { get; set; }

        /// <summary>
        /// The UUID of the preselected party. Optional.
        /// </summary>
        public Optional<Guid?> PreselectedPartyUuid { get; set; } = new();

        /// <summary>
        /// Indicates whether client units should be shown.
        /// </summary>
        public bool? ShowClientUnits { get; set; }

        /// <summary>
        /// Indicates whether sub-entities should be shown.
        /// </summary>
        public bool? ShouldShowSubEntities { get; set; }

        /// <summary>
        /// Indicates whether deleted entities should be shown.
        /// </summary>
        public bool? ShouldShowDeletedEntities { get; set; }
    }
}
