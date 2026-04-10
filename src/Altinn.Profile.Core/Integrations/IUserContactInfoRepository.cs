using Altinn.Profile.Core.User.ContactInfo;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Outlines a repository for interacting with user contact info.
/// </summary>
public interface IUserContactInfoRepository
{
    /// <summary>
    /// Updates the phone number for the given user
    /// </summary>
    /// <param name="userId">The user to update the number for</param>
    /// <param name="phoneNumber">The new phonenumber to set</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> if the phone number was updated, or null if the user was not found.</returns>
    /// <exception cref="UserContactInfoAlreadyExistsException">Thrown when a user with the same ID already exists.</exception>
    public Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Instantiates contact information for the given user
    /// </summary>
    /// <param name="userContactInfoToCreate">The user contact info to create</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> for the created user.</returns>
    public Task<UserContactInfo> CreateUserContactInfo(UserContactInfoCreateModel userContactInfoToCreate, CancellationToken cancellationToken);
}
