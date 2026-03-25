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
    /// <returns>A Task containing the <see cref="UserContactInfo"/> if the address was updated, or null if the user was not found.</returns>
    public Task<UserContactInfo?> UpdateMobileNumber(int userId, string phoneNumber);
}
