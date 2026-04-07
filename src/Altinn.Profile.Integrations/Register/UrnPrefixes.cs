using Altinn.Urn;

namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Recognized URN key prefixes used by the Register integration.
    /// This abstract partial record is annotated with <see cref="KeyValueUrnAttribute"/>
    /// and exposes helpers that check whether a URN value matches a specific key and,
    /// when applicable, returns the decoded value as <see cref="UrnEncoded"/>.
    /// </summary>
    [KeyValueUrn]
    public abstract partial record UrnPrefixes
    {
        /// <summary>
        /// Checks whether the current URN value uses the "altinn:organization:identifier-no" key and,
        /// if so, returns the decoded organization number.
        /// </summary>
        /// <param name="orgNumber">When this method returns, contains the decoded organization number as a <see cref="UrnEncoded"/>
        /// if the URN matched; otherwise contains the default value for <see cref="UrnEncoded"/>.</param>
        /// <returns><see langword="true"/> if the URN matches the organization identifier key; otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:organization:identifier-no")]
        public partial bool IsOrgNumber(out UrnEncoded orgNumber);

        /// <summary>
        /// Checks whether the current URN value uses the "altinn:party:uuid" key and,
        /// if so, returns the decoded party UUID.
        /// </summary>
        /// <param name="partyUuid">When this method returns, contains the decoded party UUID as a Guid.</param>
        /// <returns><see langword="true"/> if the URN matches the party UUID key; otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:party:uuid")]
        public partial bool IsPartyUuid(out Guid partyUuid);

        /// <summary>
        /// Checks whether the current URN value uses the "altinn:user:id" key and,
        /// if so, returns the decoded user identifier.
        /// </summary>
        /// <param name="userId">When this method returns, contains the decoded user identifier as an integer.</param>
        /// <returns><see langword="true"/> if the URN matches the user id key; otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:user:id")]
        public partial bool IsUserId(out int userId);

        /// <summary>
        /// Checks whether the current URN value uses the "altinn:party:username" key and,
        /// if so, returns the decoded username.
        /// </summary>
        /// <param name="username">When this method returns, contains the decoded username as a <see cref="UrnEncoded"/>
        /// if the URN matched; otherwise contains the default value for <see cref="UrnEncoded"/>.</param>
        /// <returns><see langword="true"/> if the URN matches the username key; otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:party:username")]
        public partial bool IsUsername(out UrnEncoded username);

        /// <summary>
        /// Checks whether the current URN value uses the "altinn:person:identifier-no" key and,
        /// if so, returns the decoded personal identifier.
        /// </summary>
        /// <param name="personId">When this method returns, contains the decoded personal identifier as a <see cref="UrnEncoded"/>
        /// if the URN matched; otherwise contains the default value for <see cref="UrnEncoded"/>.</param>
        /// <returns><see langword="true"/> if the URN matches the person identifier key; otherwise <see langword="false"/>.</returns>
        [UrnKey("altinn:person:identifier-no")]
        public partial bool IsPerson(out UrnEncoded personId);

        /// <summary>
        /// Creates a URN string for the given organization number using the "altinn:organization:identifier-no" key.
        /// </summary>
        /// <param name="orgNumber">The organization number to encode as a URN.</param>
        /// <returns>A URN string on the format <c>urn:altinn:organization:identifier-no:{orgNumber}</c>.</returns>
        public static string ToOrgNumberUrn(string orgNumber) => OrgNumber.Create(UrnEncoded.Create(orgNumber)).ToString();

        /// <summary>
        /// Creates a URN string for the given party UUID using the "altinn:party:uuid" key.
        /// </summary>
        /// <param name="partyUuid">The party UUID to encode as a URN.</param>
        /// <returns>A URN string on the format <c>urn:altinn:party:uuid:{partyUuid}</c>.</returns>
        public static string ToPartyUuidUrn(Guid partyUuid) => PartyUuid.Create(partyUuid).ToString();

        /// <summary>
        /// Creates a URN string for the given user identifier using the "altinn:user:id" key.
        /// </summary>
        /// <param name="userId">The user identifier to encode as a URN.</param>
        /// <returns>A URN string on the format <c>urn:altinn:user:id:{userId}</c>.</returns>
        public static string ToUserIdUrn(int userId) => UserId.Create(userId).ToString();

        /// <summary>
        /// Creates a URN string for the given username using the "altinn:party:username" key.
        /// </summary>
        /// <param name="username">The username to encode as a URN.</param>
        /// <returns>A URN string on the format <c>urn:altinn:party:username:{username}</c>.</returns>
        public static string ToUserNameUrn(string username) => Username.Create(UrnEncoded.Create(username)).ToString();

        /// <summary>
        /// Creates a URN string for the given national identity number using the "altinn:person:identifier-no" key.
        /// </summary>
        /// <param name="ssn">The national identity number to encode as a URN.</param>
        /// <returns>A URN string on the format <c>urn:altinn:person:identifier-no:{ssn}</c>.</returns>
        public static string ToPersonUrn(string ssn) => Person.Create(UrnEncoded.Create(ssn)).ToString();
    }
}
