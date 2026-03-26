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
    public async Task<UserContactInfo> CreateUserContactInfo(UserContactInfoCreateModel userContactInfoToCreate, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var existsRecordWithSameUserId = await databaseContext.SelfIdentifiedUsers.AnyAsync(u => u.UserId == userContactInfoToCreate.UserId, cancellationToken);
        if (existsRecordWithSameUserId)
        {
            throw new UserContactInfoAlreadyExistsException(userContactInfoToCreate.UserId);
        }

        var currentDateTime = DateTime.UtcNow;

        var userContactInfo = new UserContactInfo()
        {
            CreatedAt = currentDateTime,
            UserId = userContactInfoToCreate.UserId,
            UserUuid = userContactInfoToCreate.UserUuid,
            Username = userContactInfoToCreate.Username,
            EmailAddress = userContactInfoToCreate.EmailAddress,
            PhoneNumber = userContactInfoToCreate.PhoneNumber,
            PhoneNumberLastChanged = string.IsNullOrWhiteSpace(userContactInfoToCreate.PhoneNumber) ? null : currentDateTime
        };

        databaseContext.SelfIdentifiedUsers.Add(userContactInfo);

        await databaseContext.SaveChangesAsync(cancellationToken);

        return userContactInfo;
    }

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
