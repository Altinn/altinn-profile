using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

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

        // Check-then-insert with a catch for race conditions: The AnyAsync check handles the common case. If a concurrent insert occurs between the check and SaveChangesAsync, the PK constraint violation is caught and mapped to the same domain exception.
        var userIdAlreadyExists = await databaseContext.SelfIdentifiedUsers.AnyAsync(u => u.UserId == userContactInfoToCreate.UserId, cancellationToken);
        if (userIdAlreadyExists)
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

        try
        {
            databaseContext.SelfIdentifiedUsers.Add(userContactInfo);
            await databaseContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUserIdConflict(ex))
        {
            throw new UserContactInfoAlreadyExistsException(userContactInfoToCreate.UserId);
        }

        return userContactInfo;
    }

    private static bool IsUserIdConflict(DbUpdateException ex) => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "pk_self_identified_users" };

    /// <inheritdoc/>
    public async Task<UserContactInfo?> UpdatePhoneNumber(int userId, string phoneNumber, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var userContactInfo = await databaseContext.SelfIdentifiedUsers.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (userContactInfo == null)
        {
            return null;
        }

        bool numberHasChanged = !string.Equals(userContactInfo.PhoneNumber, phoneNumber, StringComparison.Ordinal);
        if (numberHasChanged)
        {
            userContactInfo.PhoneNumber = phoneNumber;
            userContactInfo.PhoneNumberLastChanged = DateTime.UtcNow;
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

        return userContactInfo;
    }
}
