namespace Altinn.Profile.Core.User.PortalSettings
{
    /// <summary>
    /// Provides helper methods for <see cref="LanguageType"/>.
    /// </summary>
    public static class LanguageTypeHelper
    {
        /// <summary>
        /// Gets the <see cref="LanguageType"/> corresponding to the specified type ID.
        /// </summary>
        /// <param name="id">The language type ID.</param>
        /// <returns>The corresponding <see cref="LanguageType"/>.</returns>
        public static LanguageType GetLanguageFromTypeId(int id) => id switch
        {
            1044 => LanguageType.Bokmål,
            2068 => LanguageType.Nynorsk,
            1033 => LanguageType.Engelsk,
            1083 => LanguageType.Samisk,
            _ => LanguageType.Bokmål
        };
    }
}
