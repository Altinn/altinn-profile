namespace Altinn.Profile.Integrations.SblBridge.User.Favorites
{
    /// <summary>
    /// Describes an event where a user either added or removed a party from their favorites.
    /// </summary>
    public class FavoriteChangedRequest
    {
        /// <summary>
        /// Gets or sets the type of change. Supported values are "insert" and "delete".
        /// </summary>
        public required string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the UUID of the added or removed favorite party.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the date and time for the change.
        /// </summary>
        public DateTime ChangeDateTime { get; set; }
    }
}
