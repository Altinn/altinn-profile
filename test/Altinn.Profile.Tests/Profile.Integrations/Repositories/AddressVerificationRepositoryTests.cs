using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories
{
    public class AddressVerificationRepositoryTests
    {
        private class TestDbContextFactory : IDbContextFactory<ProfileDbContext>
        {
            private readonly DbContextOptions<ProfileDbContext> _options;

            public TestDbContextFactory(DbContextOptions<ProfileDbContext> options)
            {
                _options = options;
            }

            public ProfileDbContext CreateDbContext()
            {
                return new ProfileDbContext(_options);
            }

            public Task<ProfileDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ProfileDbContext(_options));
            }
        }

        private static DbContextOptions<ProfileDbContext> CreateOptions(string DBName)
        {
            return new DbContextOptionsBuilder<ProfileDbContext>()
                .UseInMemoryDatabase(databaseName: DBName)
                .Options;
        }

        [Fact]
        public async Task AddNewVerificationCode_AddsEntityToDatabase()
        {
            var options = CreateOptions(nameof(AddNewVerificationCode_AddsEntityToDatabase));
            var factory = new TestDbContextFactory(options);
            var repository = new AddressVerificationRepository(factory);

            var verificationCode = new VerificationCode
            {
                UserId = 1,
                AddressType = AddressType.Email,
                Address = "user@example.com",
                VerificationCodeHash = "hash",
                Expires = DateTime.UtcNow.AddHours(1),
                FailedAttempts = 0
            };

            await repository.AddNewVerificationCodeAsync(verificationCode);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 1 && vc.Address == "user@example.com");
            Assert.NotNull(stored);
            Assert.Equal("hash", stored.VerificationCodeHash);
        }

        [Fact]
        public async Task TryVerifyAddress_WithMatchingHash_AddsVerifiedAddressAndRemovesCode_ReturnsTrue()
        {
            var options = CreateOptions(nameof(TryVerifyAddress_WithMatchingHash_AddsVerifiedAddressAndRemovesCode_ReturnsTrue));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                var code = new VerificationCode
                {
                    UserId = 42,
                    AddressType = AddressType.Email,
                    Address = "test@example.com",
                    VerificationCodeHash = "correct-hash",
                    Expires = DateTime.UtcNow.AddHours(1),
                    FailedAttempts = 0
                };
                seedContext.VerificationCodes.Add(code);
                await seedContext.SaveChangesAsync();
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("correct-hash", AddressType.Email, " Test@Example.com ", 42);

            Assert.True(result);

            await using var assertContext = new ProfileDbContext(options);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 42 && v.Address == "test@example.com");
            Assert.NotNull(verified);
            var remainingCode = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 42);
            Assert.Null(remainingCode);
        }

        [Fact]
        public async Task TryVerifyAddress_WithWrongHash_IncrementsFailedAttemptsAndReturnsFalse()
        {
            var options = CreateOptions(nameof(TryVerifyAddress_WithWrongHash_IncrementsFailedAttemptsAndReturnsFalse));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                var code = new VerificationCode
                {
                    UserId = 7,
                    AddressType = AddressType.Sms,
                    Address = "555-0100",
                    VerificationCodeHash = "expected-hash",
                    Expires = DateTime.UtcNow.AddHours(1),
                    FailedAttempts = 0
                };
                seedContext.VerificationCodes.Add(code);
                await seedContext.SaveChangesAsync();
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("wrong-hash", AddressType.Sms, "555-0100", 7);

            Assert.False(result);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 7 && vc.Address == "555-0100");
            Assert.NotNull(stored);
            Assert.Equal(1, stored.FailedAttempts);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 7);
            Assert.Null(verified);
        }

        [Fact]
        public async Task TryVerifyAddress_WithExpiredCode_ReturnsFalseAndDoesNotModifyCode()
        {
            var options = CreateOptions(nameof(TryVerifyAddress_WithExpiredCode_ReturnsFalseAndDoesNotModifyCode));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                var code = new VerificationCode
                {
                    UserId = 9,
                    AddressType = AddressType.Email,
                    Address = "expired@example.com",
                    VerificationCodeHash = "any-hash",
                    Expires = DateTime.UtcNow.AddHours(-1),
                    FailedAttempts = 2
                };
                seedContext.VerificationCodes.Add(code);
                await seedContext.SaveChangesAsync();
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("any-hash", AddressType.Email, "expired@example.com", 9);

            Assert.False(result);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 9 && vc.Address == "expired@example.com");
            Assert.NotNull(stored);
            Assert.Equal(2, stored.FailedAttempts);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 9);
            Assert.Null(verified);
        }

        [Fact]
        public async Task AddLegacyAddress_AddsLegacyVerifiedAddress()
        {
            var options = CreateOptions(nameof(AddLegacyAddress_AddsLegacyVerifiedAddress));
            var factory = new TestDbContextFactory(options);
            var repository = new AddressVerificationRepository(factory);

            await repository.AddLegacyAddressAsync(AddressType.Email, "legacy@example.com", 5, CancellationToken.None);

            await using var assertContext = new ProfileDbContext(options);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 5 && v.Address == "legacy@example.com");
            Assert.NotNull(verified);
            Assert.Equal(VerificationType.Legacy, verified.VerificationType);   
        }

        [Fact]
        public async Task AddLegacyAddress_WhenVerifiedAddressExists_DoesNotAddDuplicate()
        {
            var options = CreateOptions(nameof(AddLegacyAddress_WhenVerifiedAddressExists_DoesNotAddDuplicate));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                var existing = new VerifiedAddress
                {
                    UserId = 6,
                    AddressType = AddressType.Email,
                    Address = "exists@example.com",
                    VerificationType = VerificationType.Explicit
                };
                seedContext.VerifiedAddresses.Add(existing);
                await seedContext.SaveChangesAsync();
            }

            var repository = new AddressVerificationRepository(factory);

            await repository.AddLegacyAddressAsync(AddressType.Email, " exists@example.com ", 6, CancellationToken.None);

            await using var assertContext = new ProfileDbContext(options);
            var all = await assertContext.VerifiedAddresses.Where(v => v.UserId == 6 && v.Address == "exists@example.com").ToListAsync();
            Assert.Single(all);
            Assert.Equal(VerificationType.Explicit, all[0].VerificationType);
        }
    }
}
