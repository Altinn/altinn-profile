using System;
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

            await repository.AddNewVerificationCode(verificationCode);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 1 && vc.Address == "user@example.com", cancellationToken: TestContext.Current.CancellationToken);
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
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("correct-hash", AddressType.Email, " Test@Example.com ", 42);

            Assert.True(result);

            await using var assertContext = new ProfileDbContext(options);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 42 && v.Address == "test@example.com", cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(verified);
            var remainingCode = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 42, cancellationToken: TestContext.Current.CancellationToken);
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
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("wrong-hash", AddressType.Sms, "555-0100", 7);

            Assert.False(result);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 7 && vc.Address == "555-0100", cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(stored);
            Assert.Equal(1, stored.FailedAttempts);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 7, cancellationToken: TestContext.Current.CancellationToken);
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
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.TryVerifyAddress("any-hash", AddressType.Email, "expired@example.com", 9);

            Assert.False(result);

            await using var assertContext = new ProfileDbContext(options);
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 9 && vc.Address == "expired@example.com", cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(stored);
            Assert.Equal(2, stored.FailedAttempts);
            var verified = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(v => v.UserId == 9, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Null(verified);
        }

        [Fact]
        public async Task GetVerificationStatus_WhenVerificationCodeExists_ReturnsUnverified()
        {
            var options = CreateOptions(nameof(GetVerificationStatus_WhenVerificationCodeExists_ReturnsUnverified));
            var factory = new TestDbContextFactory(options);
            await using (var seedContext = new ProfileDbContext(options))
            {
                var code = new VerificationCode
                {
                    UserId = 1,
                    AddressType = AddressType.Email,
                    Address = "not-verified@test.com",
                    VerificationCodeHash = "any-hash",
                    Expires = DateTime.UtcNow.AddHours(-1),
                    FailedAttempts = 2
                };
                seedContext.VerificationCodes.Add(code);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationStatusAsync(1, AddressType.Email, "not-verified@test.com", CancellationToken.None);

            Assert.Equal(VerificationType.Unverified, result);
        }

        [Fact]
        public async Task GetVerificationStatus_WhenNeitherVerifiedOrCodeExists_ReturnsLegacy()
        {
            var options = CreateOptions(nameof(GetVerificationStatus_WhenNeitherVerifiedOrCodeExists_ReturnsLegacy));
            var factory = new TestDbContextFactory(options);

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationStatusAsync(1, AddressType.Email, "not-verified@test.com", CancellationToken.None);

            Assert.Equal(VerificationType.Legacy, result);
        }

        [Fact]
        public async Task GetVerificationStatus_WhenVerifiedEmail_ReturnsVerificationStatus()
        {
            var options = CreateOptions(nameof(GetVerificationStatus_WhenVerifiedEmail_ReturnsVerificationStatus));
            var factory = new TestDbContextFactory(options);
            await using (var seedContext = new ProfileDbContext(options))
            {
                var verifiedAddress = new VerifiedAddress
                {
                    UserId = 9,
                    AddressType = AddressType.Email,
                    Address = "verified@example.com",
                    VerificationType = VerificationType.Verified,
                };
                seedContext.VerifiedAddresses.Add(verifiedAddress);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationStatusAsync(9, AddressType.Email, "Verified@example.com ", CancellationToken.None);

            Assert.Equal(VerificationType.Verified, result);
        }

        [Fact]
        public async Task GetVerificationStatus_WhenVerifiedSms_ReturnsVerificationStatus()
        {
            var options = CreateOptions(nameof(GetVerificationStatus_WhenVerifiedSms_ReturnsVerificationStatus));
            var factory = new TestDbContextFactory(options);
            await using (var seedContext = new ProfileDbContext(options))
            {
                var verifiedAddress = new VerifiedAddress
                {
                    UserId = 9,
                    AddressType = AddressType.Sms,
                    Address = "+4799999999",
                    VerificationType = VerificationType.Legacy,
                };
                seedContext.VerifiedAddresses.Add(verifiedAddress);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationStatusAsync(9, AddressType.Sms, " +4799999999 ", CancellationToken.None);

            Assert.Equal(VerificationType.Legacy, result);
        }

        [Fact]
        public async Task GetVerifiedAddresses_WhenNone_ReturnsEmptyList()
        {
            var options = CreateOptions(nameof(GetVerifiedAddresses_WhenNone_ReturnsEmptyList));
            var factory = new TestDbContextFactory(options);

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerifiedAddressesAsync(1, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetVerifiedAddresses_WhenThereAreVerifiedAddresses_ReturnsList()
        {
            var options = CreateOptions(nameof(GetVerifiedAddresses_WhenThereAreVerifiedAddresses_ReturnsList));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                seedContext.VerifiedAddresses.Add(new VerifiedAddress
                {
                    UserId = 9,
                    AddressType = AddressType.Email,
                    Address = "verified1@example.com",
                    VerificationType = VerificationType.Verified,
                });

                seedContext.VerifiedAddresses.Add(new VerifiedAddress
                {
                    UserId = 9,
                    AddressType = AddressType.Sms,
                    Address = "+4790000001",
                    VerificationType = VerificationType.Legacy,
                });

                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerifiedAddressesAsync(9, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, v => v.Address == "verified1@example.com" && v.AddressType == AddressType.Email && v.VerificationType == VerificationType.Verified);
            Assert.Contains(result, v => v.Address == "+4790000001" && v.AddressType == AddressType.Sms && v.VerificationType == VerificationType.Legacy);
        }
    }
}
