namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing user contact information.
    /// </summary>
    public interface IUserContactInfoService
    {
        /// <summary>
        /// Updates the phone number for the given user
        /// </summary>
        /// <param name="userId">The user to update the number for</param>
        /// <param name="phoneNumber">The new phonenumber to set</param>
        /// <param name="cancellationToken">Cancel the current request</param>
        /// <returns>The updated UserContactInfo or null if the update failed.</returns>
        Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken);
    }
}
