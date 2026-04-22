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
    /// <param name="phoneNumber">The new phonenumber to set. Use <see langword="null"/> to clear the phone number.</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> if the phone number was updated, or null if the user was not found.</returns>
    public Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Instantiates contact information for the given user
    /// </summary>
    /// <param name="userContactInfoToCreate">The user contact info to create</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> for the created user.</returns>
    /// <exception cref="UserContactInfoAlreadyExistsException">Thrown when a user with the same ID already exists.</exception>
    public Task<UserContactInfo> CreateUserContactInfo(UserContactInfoCreateModel userContactInfoToCreate, CancellationToken cancellationToken);

    /// <summary>
    /// Get the contact information for a given user. Returns null if no contact information is found for the user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve contact information for.</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> if found, or null if no contact information is found for the user.</returns>
    public Task<UserContactInfo?> Get(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the contact information for a given user by username. Returns null if no contact information is found for the user.
    /// </summary>
    /// <param name="username">The username of the user to retrieve contact information for.</param>
    /// <param name="cancellationToken">Cancel the current request</param>
    /// <returns>A Task containing the <see cref="UserContactInfo"/> if found, or null if no contact information is found for the user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when multiple records exist with the same username.</exception>
    public Task<UserContactInfo?> GetByUsername(string username, CancellationToken cancellationToken);
}
