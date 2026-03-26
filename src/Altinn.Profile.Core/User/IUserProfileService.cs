using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Interface handling methods for operations related to user profiles
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Method that fetches a user based on a user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User profile with given user id or a boolean if failure.</returns>
    Task<Result<UserProfile, bool>> GetUser(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Method that fetches a user based on ssn.
    /// </summary>
    /// <param name="ssn">The user's ssn.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User profile connected to given ssn or a boolean if failure.</returns>
    Task<Result<UserProfile, bool>> GetUser(string ssn, CancellationToken cancellationToken);

    /// <summary>
    /// Method that fetches a user based on a user uuid
    /// </summary>
    /// <param name="userUuid">The user uuid</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User profile with given user uuid or a boolean if failure.</returns>
    Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid, CancellationToken cancellationToken);

    /// <summary>
    /// Method that fetches a list of users based on a list of user uuid
    /// </summary>
    /// <param name="userUuidList">The list of user uuids</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>List of User profiles with given user uuids or a boolean if failure.</returns>
    Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList, CancellationToken cancellationToken);

    /// <summary>
    /// Method that fetches a user based on username.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User profile connected to given username or a boolean if failure.</returns>
    Task<Result<UserProfile, bool>> GetUserByUsername(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the profile settings for a user.
    /// </summary>
    /// <param name="profileSettings">The updated profile settings from request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings, CancellationToken cancellationToken);

    /// <summary>
    /// Patches the profile settings for a user.
    /// </summary>
    /// <param name="profileSettings">The updated profile settings from request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<ProfileSettings.ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the profile's preferred language (or, if not set, a default) for a given user ID.
    /// </summary>
    Task<string> GetPreferredLanguage(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the timestamp for the event (if it has occurred, otherwise null) when the user chose to ignore updates to unit profiles.
    /// </summary>
    Task<DateTime?> GetIgnoreUnitProfileDateTime(int userId, CancellationToken cancellationToken = default);
}
