using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="UserContactInfoRepository"/> class.
/// </summary>
/// <param name="contextFactory">A factory for creating instances of <see cref="ProfileDbContext"/></param>
public class UserContactInfoRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IUserContactInfoRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <inheritdoc/>
    public async Task<UserContactInfo?> UpdatePhoneNumber(int userId, string phoneNumber, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var userContactInfo = await databaseContext.SelfIdentifiedUsers.FirstOrDefaultAsync(u => u.UserId.Equals(userId), cancellationToken);
        if (userContactInfo == null)
        {
            return null;
        }

        userContactInfo.PhoneNumber = phoneNumber;
        userContactInfo.PhoneNumberLastChanged = DateTime.UtcNow;

        await databaseContext.SaveChangesAsync(cancellationToken);
        return userContactInfo;
    }
}
