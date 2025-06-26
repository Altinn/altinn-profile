﻿namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// A set of identifiers for a party.
    /// </summary>
    public record PartyIdentifiersResponse
    {
        /// <summary>
        /// The party id.
        /// </summary>
        public int PartyId { get; init; }

        /// <summary>
        /// The party uuid.
        /// </summary>
        public Guid PartyUuid { get; init; }
    }
}
