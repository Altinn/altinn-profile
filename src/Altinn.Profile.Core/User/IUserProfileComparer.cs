using Altinn.Profile.Models;

using static Altinn.Profile.Core.User.UserProfileComparer;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Defines comparison functionality for user profile data during shadow/parallell reads.
/// </summary>
public interface IUserProfileComparer
{
    /// <summary>
    /// Compares two <see cref="UserProfile"/> instances, logs detected mismatches, and returns all mismatches.
    /// </summary>
    /// <param name="source">The source profile.</param>
    /// <param name="target">The target profile.</param>
    /// <returns>A list of mismatches discovered during comparison.</returns>
    IReadOnlyList<UserProfileMismatch> CompareAndLog(UserProfile? source, UserProfile? target);
}
