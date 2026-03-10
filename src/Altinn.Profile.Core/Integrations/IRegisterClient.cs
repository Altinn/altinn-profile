using Altinn.Register.Contracts;

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
        /// Get partyId for a party based on its UUID.
        /// </summary>
        /// <param name="partyUuid" >The UUID of the party to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        Task<int?> GetPartyId(Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Get party UUIDs for a list of organization numbers.
        /// </summary>
        /// <param name="orgNumbers">The organization numbers to look up</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IReadOnlyList<Organization>?> GetPartyUuids(string[] orgNumbers, CancellationToken cancellationToken);

        /// <summary>
        /// Get organization number for a party based on its UUID.
        /// </summary>
        /// <param name="partyUuid">The partyUuid to look up</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string?> GetOrganizationNumberByPartyUuid(Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Get party information for a user based on their UUID.
        /// </summary>
        /// <param name="userUuid">The UUID of the user to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, carrying the value of <see cref="Register.Contracts.Party"/> matching the given user UUID, or null if no match was found.</returns>
        Task<Register.Contracts.Party?> GetUserParty(Guid userUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Get party information for a user based on their user ID.
        /// </summary>
        /// <param name="userId">The user ID to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, carrying the value of <see cref="Register.Contracts.Party"/> matching the given userId, or null if no match was found.</returns>
        Task<Register.Contracts.Party?> GetUserParty(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// Get party information for a user based on their username.
        /// </summary>
        /// <param name="username">The username to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, carrying the value of <see cref="Register.Contracts.Party"/> matching the given username, or null if no match was found.</returns>
        Task<Register.Contracts.Party?> GetUserPartyByUsername(string username, CancellationToken cancellationToken);

        /// <summary>
        /// Get party information for a user based on their national identification number (SSN).
        /// </summary>
        /// <param name="ssn">The national identification number to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, carrying the value of <see cref="Register.Contracts.Party"/> matching the given user SSN, or null if no match was found.</returns>
        Task<Register.Contracts.Party?> GetUserPartyBySsn(string ssn, CancellationToken cancellationToken);

        /// <summary>
        /// Get party information for multiple users based on their UUIDs.
        /// </summary>
        /// <param name="userUuids">The list of user UUIDs to look up.</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, carrying a list of <see cref="Register.Contracts.Party"/> matching the given user UUIDs, or empty list if no match was found.</returns>
        Task<IReadOnlyList<Register.Contracts.Party>> GetUserParties(List<Guid> userUuids, CancellationToken cancellationToken);
    }
}
