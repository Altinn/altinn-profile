namespace Altinn.Profile.Integrations.Handlers
{
    /// <summary>
    /// Provides constants for different types of changes.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public static class ChangeType
    {
        /// <summary>
        /// Represents an insert change type.
        /// </summary>
        public const string Insert = "insert";

        /// <summary>
        /// Represents an update change type.
        /// </summary>
        public const string Update = "update";

        /// <summary>
        /// Represents a delete change type.
        /// </summary>
        public const string Delete = "delete";
    }
}
