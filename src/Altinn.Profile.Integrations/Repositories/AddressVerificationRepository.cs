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
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task AddNewVerificationCode(VerificationCode verificationCode)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var verificationCodes = await databaseContext.VerificationCodes.Where(vc => vc.UserId.Equals(verificationCode.UserId) && vc.AddressType == verificationCode.AddressType && vc.Address == verificationCode.Address).ToListAsync();

        databaseContext.VerificationCodes.RemoveRange(verificationCodes);

        databaseContext.VerificationCodes.Add(verificationCode);
        await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Tries to verify an address using the provided verification code hash.
    /// </summary>
    /// <param name="verificationCodeHash">The hash to compare</param>
    /// <param name="addressType">If the address is for sms or email</param>
    /// <param name="address">The address to verify</param>
    /// <param name="userId">The id of the user</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> TryVerifyAddress(string verificationCodeHash, AddressType addressType, string address, int userId)
    {
        var verified = false;
        address = address.Trim().ToLowerInvariant();

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var verificationCode = await databaseContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == address);
        
        if (verificationCode == null || verificationCode.Expires < DateTime.UtcNow)
        {
            return verified;
        }

        if (verificationCode.VerificationCodeHash == verificationCodeHash)
        {
            var verifiedAddress = new VerifiedAddress
            {
                UserId = userId,
                AddressType = addressType,
                Address = address,
            };
            databaseContext.VerifiedAddresses.Add(verifiedAddress);
            databaseContext.VerificationCodes.Remove(verificationCode);
            verified = true;
        }
        else
        {
            verificationCode.FailedAttempts += 1;
            databaseContext.VerificationCodes.Update(verificationCode);
        }

        await databaseContext.SaveChangesAsync();
        return verified;
    }

    /// <inheritdoc />
    public async Task<VerificationType?> GetVerificationStatus(int userId, AddressType addressType, string address, CancellationToken cancellationToken)
    {
        var addressCleaned = address.Trim().ToLowerInvariant();

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var verifiedAddresses = await databaseContext.VerifiedAddresses.Where(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == addressCleaned)
            .AsNoTracking().ToListAsync(cancellationToken);

        if (verifiedAddresses.Count == 0)
        {
            return null;
        }

        return verifiedAddresses[0].VerificationType;
    }
}
