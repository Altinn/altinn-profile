namespace Altinn.Profile.Core.PartyGroups
{
    /// <summary>
    /// Represents the result of an update group operation.
    /// </summary>
    /// <param name="Result">The operation result status.</param>
    /// <param name="Group">The updated group, if the operation was successful.</param>
    public record UpdateGroupResult(GroupOperationResult Result, Group? Group);
}
