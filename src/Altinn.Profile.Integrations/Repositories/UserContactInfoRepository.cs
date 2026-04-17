using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Wolverine.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="UserContactInfoRepository"/> class.
/// </summary>
/// <param name="contextFactory">A factory for creating instances of <see cref="ProfileDbContext"/></param>
/// <param name="databaseContextOutbox">The outbox for handling transactional operations</param>
public class UserContactInfoRepository(IDbContextFactory<ProfileDbContext> contextFactory, IDbContextOutbox databaseContextOutbox) : EFCoreTransactionalOutbox(databaseContextOutbox), IUserContactInfoRepository
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

            SiUserContactInfoAddedEvent NotifySiUserContactInfoAdded() => new(userContactInfo.UserId, currentDateTime, userContactInfo.EmailAddress, userContactInfo.PhoneNumber);
            await NotifyAndSave(databaseContext, NotifySiUserContactInfoAdded, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUserIdConflict(ex))
        {
            throw new UserContactInfoAlreadyExistsException(userContactInfoToCreate.UserId);
        }

        return userContactInfo;
    }

    private static bool IsUserIdConflict(DbUpdateException ex) => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "pk_self_identified_users" };

    /// <inheritdoc/>
    public async Task<UserContactInfo?> UpdatePhoneNumber(int userId, string? phoneNumber, CancellationToken cancellationToken)
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
            var currentDateTime = DateTime.UtcNow;
            userContactInfo.PhoneNumber = phoneNumber;
            userContactInfo.PhoneNumberLastChanged = currentDateTime;

            // Empty string is used to indicate removal of phone number
            SiUserContactInfoUpdatedEvent NotifySiUserContactInfoUpdated() => new(userContactInfo.UserId, currentDateTime, userContactInfo.EmailAddress, userContactInfo.PhoneNumber ?? string.Empty);
            await NotifyAndSave(databaseContext, NotifySiUserContactInfoUpdated, cancellationToken);
        }

        return userContactInfo;
    }

    /// <inheritdoc/>
    public async Task<UserContactInfo?> Get(int userId, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var userContactInfo = await databaseContext.SelfIdentifiedUsers.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        return userContactInfo;
    }

    /// <inheritdoc/>
    public async Task<UserContactInfo?> GetByUsername(string username, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var userContactInfo = await databaseContext.SelfIdentifiedUsers.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        return userContactInfo;
    }
}
