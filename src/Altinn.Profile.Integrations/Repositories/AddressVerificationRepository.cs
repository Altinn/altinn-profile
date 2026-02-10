#nullable enable
using System;
using System.Reflection;
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
    public async Task AddNewVerificationCodeAsync(VerificationCode verificationCode)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var verificationCodes = await databaseContext.VerificationCodes.Where(vc => vc.UserId.Equals(verificationCode.UserId) && vc.AddressType == verificationCode.AddressType && vc.Address == verificationCode.Address).ToListAsync();

        databaseContext.VerificationCodes.RemoveRange(verificationCodes);

        databaseContext.VerificationCodes.Add(verificationCode);
        await databaseContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> TryVerifyAddressAsync(int userId, AddressType addressType, string address, Func<string, bool> verifyFunc, CancellationToken cancellationToken)
    {
        var verified = false;
        address = VerificationCode.FormatAddress(address);

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var verificationCode = await databaseContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == address, cancellationToken);
        
        if (verificationCode == null || verificationCode.Expires < DateTime.UtcNow)
        {
            return verified;
        }

        if (verifyFunc(verificationCode.VerificationCodeHash))
        {
            var verifiedAddress = new VerifiedAddress
            {
                UserId = userId,
                AddressType = addressType,
                Address = address,
            };
            var existingVerifications = databaseContext.VerifiedAddresses.Where(va => va.UserId.Equals(userId) && va.AddressType == addressType && va.Address == address).ToArray();
            databaseContext.VerifiedAddresses.RemoveRange(existingVerifications);

            databaseContext.VerifiedAddresses.Add(verifiedAddress);
            databaseContext.VerificationCodes.Remove(verificationCode);
            verified = true;
        }
        else
        {
            verificationCode.FailedAttempts += 1;
            databaseContext.VerificationCodes.Update(verificationCode);
        }

        await databaseContext.SaveChangesAsync(cancellationToken);
        return verified;
    }

    /// <inheritdoc/>
    public async Task AddLegacyAddressAsync(AddressType addressType, string address, int userId, CancellationToken cancellationToken)
    {
        address = VerificationCode.FormatAddress(address);

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var verifiedAddress = await databaseContext.VerifiedAddresses.FirstOrDefaultAsync(vc => vc.UserId.Equals(userId) && vc.AddressType == addressType && vc.Address == address, cancellationToken);

        if (verifiedAddress != null)
        {
            return;
        }
        
        verifiedAddress = new VerifiedAddress
        {
            UserId = userId,
            AddressType = addressType,
            Address = address,
            VerificationType = VerificationType.Legacy
        };
        databaseContext.VerifiedAddresses.Add(verifiedAddress);

        try
        {
            await databaseContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Accept unique-constraint violations from Postgres (23505).
            // Some tests simulate a PostgresException by providing a different exception type that exposes a SqlState property.
            var inner = ex.InnerException;
            if (inner != null)
            {               
                var prop = inner.GetType().GetProperty("SqlState", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    var val = prop.GetValue(inner) as string;
                    if (val == "23505")
                    {
                        return;
                    }
                }
            }

            // If it's a different kind of DbUpdateException, re-throw
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<VerificationType?> GetVerificationStatusAsync(int userId, AddressType addressType, string address, CancellationToken cancellationToken)
    {
        var addressCleaned = VerificationCode.FormatAddress(address);

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
