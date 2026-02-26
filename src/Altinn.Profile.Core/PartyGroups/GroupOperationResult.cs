namespace Altinn.Profile.Core.PartyGroups
{
    /// <summary>
    /// Represents the result of a party group operation.
    /// </summary>
    public enum GroupOperationResult
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The group was not found or the user does not have access to it.
        /// </summary>
        NotFound,

        /// <summary>
        /// The operation is not allowed due to business rules (e.g., favorite groups cannot be modified).
        /// </summary>
        Forbidden
    }
}
