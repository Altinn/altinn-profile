namespace Altinn.Profile.Core.User.ProfileSettings
{
    /// <summary>
    /// Represents the supported language types for the user portal.
    /// </summary>
    public class LanguageType
    {
        /// <summary>
        /// Norwegian Bokmål language.
        /// </summary>
        public const string NB = "nb";

        /// <summary>
        /// Norwegian Nynorsk language.
        /// </summary>
        public const string NN = "nn";

        /// <summary>
        /// English language.
        /// </summary>
        public const string EN = "en";

        /// <summary>
        /// Gets the language code corresponding to the specified Altinn 2 language code.
        /// </summary>
        /// <param name="altinn2Code">The Altinn 2 language code (e.g., 1044 for Norwegian Bokmål, 2068 for Norwegian Nynorsk, 1033 for English).</param>
        /// <returns>The language code as a string ("nb", "nn", or "en"). Defaults to "nb" if the code is not recognized.</returns>
        public static string GetFromAltinn2Code(int altinn2Code)
        {
            return altinn2Code switch
            {
                1044 => NB,
                2068 => NN,
                1033 => EN,
                _ => NB
            };
        }
    }
}
