namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// A set of identifiers for a party.
    /// </summary>
    public record PartyIdentifiersResponse
    {
        /// <summary>
        /// The party id.
        /// </summary>
        public required int PartyId { get; init; }

        /// <summary>
        /// The party uuid.
        /// </summary>
        public required Guid PartyUuid { get; init; }
    }
}
