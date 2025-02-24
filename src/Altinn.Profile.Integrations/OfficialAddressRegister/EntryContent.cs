using System.Text.Json.Serialization;

namespace Altinn.Profile.Integrations.OfficialAddressRegister
{
    /// <summary>
    /// Gets the content of the cotanct point.
    /// </summary>
    public record EntryContent
    {
        /// <summary>
        /// Gets the content of the cotanct point.
        /// </summary>
        [JsonPropertyName("Kontaktinformasjon")]
        public ContactPoint? Kontaktinformasjon { get; init; }

        /// <summary>
        /// Gets the content of the cotanct point.
        /// </summary>
        public record ContactPoint
        {
            /// <summary>
            /// Gets the identificator of the cotanct point.
            /// </summary>
            [JsonPropertyName("identifikator")]
            public string? Id { get; init; }

            /// <summary>
            /// Gets the content of the contact point.
            /// </summary>
            [JsonPropertyName("digitalVarslingsinformasjon")]
            public DigitalContactPointModel? DigitalContactPoint { get; init; }

            /// <summary>
            /// Gets the content of the contact point.
            /// </summary>
            [JsonPropertyName("kontaktinformasjonForEnhet")]
            public UnitContactInfoModel? UnitContactInfo { get; init; }

            /// <summary>
            /// Gets the content of the contact point.
            /// </summary>
            public record DigitalContactPointModel
            {
                /// <summary>
                /// Gets the email address of the contact point.
                /// </summary>
                [JsonPropertyName("epostadresse")]
                public EmailAddressModel? EmailAddress { get; init; }

                /// <summary>
                /// Gets the mobile phone number of the contact point.
                /// </summary>
                [JsonPropertyName("mobiltelefon")]
                public PhoneNumberModel? PhoneNumber { get; init; }

                /// <summary>
                /// Gets the full the email address model.
                /// </summary>
                public record EmailAddressModel
                {
                    /// <summary>
                    /// Gets the full the email address.
                    /// </summary>
                    [JsonPropertyName("navn")]
                    public string? Name { get; init; }

                    /// <summary>
                    /// Gets the domain name of the email address.
                    /// </summary>
                    [JsonPropertyName("domenenavn")]
                    public string? Domain { get; init; }

                    /// <summary>
                    /// Gets the username of the email address.
                    /// </summary>
                    [JsonPropertyName("brukernavn")]
                    public string? Username { get; init; }
                }

                /// <summary>
                /// Gets the full the phone number model.
                /// </summary>
                public record PhoneNumberModel
                {
                    /// <summary>
                    /// Gets the full phone number.
                    /// </summary>
                    [JsonPropertyName("navn")]
                    public string? Number { get; init; }

                    /// <summary>
                    /// Gets the international prefix of the phone number.
                    /// </summary>
                    [JsonPropertyName("internasjonaltPrefiks")]
                    public string? Domain { get; init; }

                    /// <summary>
                    /// Gets the national phone number.
                    /// </summary>
                    [JsonPropertyName("nasjonaltNummer")]
                    public string? NationalNumber { get; init; }
                }
            }

            /// <summary>
            /// Gets the identifier of the unit of the contact point.
            /// </summary>
            public record UnitContactInfoModel
            {
                /// <summary>
                /// Gets the identifier of the unit of the contact point.
                /// </summary>
                [JsonPropertyName("enhetsidentifikator")]
                public UnitIdentifierModel? UnitIdentifier { get; init; }

                /// <summary>
                /// Gets the identificator of the unit.
                /// </summary>
                public record UnitIdentifierModel
                {
                    /// <summary>
                    /// Gets the value of the identificator of the cotanct point.
                    /// </summary>
                    [JsonPropertyName("verdi")]
                    public string? Value { get; init; }

                    /// <summary>
                    /// Gets the type of identifier.
                    /// </summary>
                    [JsonPropertyName("type")]
                    public string? Type { get; init; }
                }
            }
        }
    }
}
