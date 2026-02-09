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

    /// <summary>
    /// Tries to verify an address using the provided verification code hash.
    /// </summary>
    /// <param name="verificationCodeHash">The hash to compare</param>
    /// <param name="addressType">If the address is for sms or email</param>
    /// <param name="address">The address to verify</param>
    /// <param name="userId">The id of the user</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> TryVerifyAddressAsync(string verificationCodeHash, AddressType addressType, string address, int userId)
    {
        var verified = false;
        address = VerificationCode.FormatAddress(address);

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
                // Directly handle real Npgsql.PostgresException
                if (inner.GetType().FullName == "Npgsql.PostgresException")
                {
                    // use reflection to read SqlState to avoid hard dependency on Npgsql at runtime in tests
                    var sqlStateProp = inner.GetType().GetProperty("SqlState", BindingFlags.Public | BindingFlags.Instance);
                    if (sqlStateProp?.GetValue(inner) as string == "23505")
                    {
                        return;
                    }
                }

                // Fallback: any exception exposing a public SqlState string property with value "23505"
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
}
