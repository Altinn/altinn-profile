namespace Altinn.Profile.Core.User.ContactInfo;

/// <summary>
/// Exception thrown when attempting to create a <see cref="UserContactInfo"/> for a user that already exists.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserContactInfoAlreadyExistsException"/> class.
/// </remarks>
/// <param name="userId">The user ID that already exists.</param>
public class UserContactInfoAlreadyExistsException(int userId) : Exception($"User contact info for user {userId} already exists.")
{
}
