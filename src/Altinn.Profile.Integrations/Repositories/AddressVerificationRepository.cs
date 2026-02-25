#nullable enable
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for operations related to address verification.
/// </summary>
public class AddressVerificationRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IAddressVerificationRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <summary>
    /// Adds a new verification code to the database.
    /// </summary>
    /// <param name="verificationCode">The verification code to add.</param>
    /// <returns><c>true</c> if the verification code was successfully added; <c>false</c> if it was discarded due to a concurrent insert.</returns>
    public async Task<bool> AddNewVerificationCodeAsync(VerificationCode verificationCode)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        try
        {
            var verificationCodes = databaseContext.VerificationCodes.Where(vc => vc.UserId.Equals(verificationCode.UserId) && vc.AddressType == verificationCode.AddressType && vc.Address == verificationCode.Address);

            // Remove any existing verification codes for the same user and address before adding the new one
            databaseContext.VerificationCodes.RemoveRange(verificationCodes);

            databaseContext.VerificationCodes.Add(verificationCode);
            await databaseContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<VerificationCode?> GetVerificationCodeAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var verificationCode = await databaseContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == address, cancellationToken);
        return verificationCode;
    }

    /// <inheritdoc/>
    public async Task CompleteAddressVerificationAsync(int verificationCodeId, AddressType addressType, string address, int userId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var verifiedAddress = new VerifiedAddress
        {
            UserId = userId,
            AddressType = addressType,
            Address = address,
        };

        var codeToRemove = await databaseContext.VerificationCodes.FindAsync(verificationCodeId);
        if (codeToRemove is null)
        {
            // This might indicate that the verification code has already been verified in another semi-concurrent process.
            // In that case, we can safely ignore the request to complete the verification as the address is already verified.
            return;
        }

        databaseContext.VerificationCodes.Remove(codeToRemove);
        databaseContext.VerifiedAddresses.Add(verifiedAddress);

        await databaseContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task IncrementFailedAttemptsAsync(int verificationCodeId)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var verificationCode = await databaseContext.VerificationCodes.FindAsync(verificationCodeId);
        if (verificationCode is not null)
        {
            verificationCode.IncrementFailedAttempts();
            await databaseContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<VerificationType> GetVerificationStatusAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken)
    {
        var addressCleaned = VerificationCode.FormatAddress(address);

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var verifiedAddress = await databaseContext.VerifiedAddresses.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == addressCleaned, cancellationToken);

        if (verifiedAddress != null)
        {
            return VerificationType.Verified;
        }

        var verificationCode = await databaseContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == addressCleaned, cancellationToken);
        if (verificationCode != null)
        {
            return VerificationType.Unverified;
        }

        return VerificationType.Legacy;
    }

    /// <inheritdoc />
    public async Task<List<VerifiedAddress>> GetVerifiedAddressesAsync(int userId, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var verifiedAddresses = await databaseContext.VerifiedAddresses.Where(va => va.UserId.Equals(userId))
            .AsNoTracking().ToListAsync(cancellationToken);

        return verifiedAddresses;
    }
}
