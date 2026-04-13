namespace Altinn.Profile.Core.User.ContactInfo
{
    /// <summary>
    /// Service for managing user contact information.
    /// </summary>
    public interface IUserContactInfoService
    {
        /// <summary>
        /// Checks if the phone number has been verified, or if the phone number is null. If the phone number is null, this method returns true, as there is no phone number to verify.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="phoneNumber">The phone number to check.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the phone number is verified or null, otherwise false.</returns>
        Task<bool> IsAddressVerifiedOrNull(int userId, string phoneNumber, CancellationToken cancellationToken);

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
