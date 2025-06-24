namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// A http client for interactions with altinn-register
    /// </summary>
    public interface IRegisterClient
    {
        /// <summary>
        /// Optimistically get the main unit for an organization based on its organization number.
        /// </summary>
        /// <param name="orgNumber">Org number for the organization to look up a potential main unit for.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string?> GetMainUnit(string orgNumber, CancellationToken cancellationToken);

        /// <summary>
        /// Get prtyId for a party based on its UUID.
        /// </summary>
        /// <param name="partyUuid" >The UUID of the party to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        Task<int?> GetPartyId(Guid partyUuid, CancellationToken cancellationToken);
    }
}
