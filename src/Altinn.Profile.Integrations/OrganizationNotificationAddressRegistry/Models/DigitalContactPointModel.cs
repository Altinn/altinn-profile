using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models
{
    /// <summary>
    /// The content of the contact point.
    /// </summary>
    public record DigitalContactPointModel
    {
        /// <summary>
        /// The email address of the contact point.
        /// </summary>
        [JsonPropertyName("epostadresse")]
        public EmailAddressModel? EmailAddress { get; init; }

        /// <summary>
        /// The mobile phone number of the contact point.
        /// </summary>
        [JsonPropertyName("mobiltelefon")]
        public PhoneNumberModel? PhoneNumber { get; init; }

        /// <summary>
        /// The full the email address model.
        /// </summary>
        public record EmailAddressModel
        {
            /// <summary>
            /// The full the email address.
            /// </summary>
            [JsonPropertyName("navn")]
            public string? Name { get; init; }

            /// <summary>
            /// The domain name of the email address.
            /// </summary>
            [JsonPropertyName("domenenavn")]
            public string? Domain { get; init; }

            /// <summary>
            /// The username of the email address.
            /// </summary>
            [JsonPropertyName("brukernavn")]
            public string? Username { get; init; }
        }

        /// <summary>
        /// The full the phone number model.
        /// </summary>
        public record PhoneNumberModel
        {
            /// <summary>
            /// The full phone number.
            /// </summary>
            [JsonPropertyName("navn")]
            public string? Name { get; init; }

            /// <summary>
            /// The international prefix of the phone number.
            /// </summary>
            [JsonPropertyName("internasjonaltPrefiks")]
            public string? Prefix { get; init; }

            /// <summary>
            /// The national phone number.
            /// </summary>
            [JsonPropertyName("nasjonaltNummer")]
            public string? NationalNumber { get; init; }
        }
    }
}
