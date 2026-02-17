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
            var stored = await assertContext.VerificationCodes.FirstOrDefaultAsync(vc => vc.UserId == 1 && vc.Address == "user@example.com", cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(stored);
            Assert.Equal("hash", stored.VerificationCodeHash);
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
                    VerificationType = VerificationType.Verified,
                };
                seedContext.VerifiedAddresses.Add(verifiedAddress);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationStatusAsync(9, AddressType.Sms, " +4799999999 ", CancellationToken.None);

            Assert.Equal(VerificationType.Verified, result);
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
                    VerificationType = VerificationType.Verified,
                });

                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerifiedAddressesAsync(9, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, v => v.Address == "verified1@example.com" && v.AddressType == AddressType.Email && v.VerificationType == VerificationType.Verified);
            Assert.Contains(result, v => v.Address == "+4790000001" && v.AddressType == AddressType.Sms && v.VerificationType == VerificationType.Verified);
        }

        [Fact]
        public async Task GetVerificationCode_WhenCodeExists_ReturnsVerificationCode()
        {
            var options = CreateOptions(nameof(GetVerificationCode_WhenCodeExists_ReturnsVerificationCode));
            var factory = new TestDbContextFactory(options);

            await using (var seedContext = new ProfileDbContext(options))
            {
                var code = new VerificationCode
                {
                    UserId = 5,
                    AddressType = AddressType.Email,
                    Address = "test@example.com",
                    VerificationCodeHash = "test-hash-123",
                    Expires = DateTime.UtcNow.AddHours(1),
                    FailedAttempts = 0
                };
                seedContext.VerificationCodes.Add(code);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationCodeAsync(5, AddressType.Email, "test@example.com", CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(5, result.UserId);
            Assert.Equal(AddressType.Email, result.AddressType);
            Assert.Equal("test@example.com", result.Address);
            Assert.Equal("test-hash-123", result.VerificationCodeHash);
            Assert.Equal(0, result.FailedAttempts);
        }

        [Fact]
        public async Task GetVerificationCode_WhenCodeDoesNotExist_ReturnsNull()
        {
            var options = CreateOptions(nameof(GetVerificationCode_WhenCodeDoesNotExist_ReturnsNull));
            var factory = new TestDbContextFactory(options);

            var repository = new AddressVerificationRepository(factory);

            var result = await repository.GetVerificationCodeAsync(999, AddressType.Email, "nonexistent@example.com", CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task CompleteAddressVerification_AddsVerifiedAddressAndRemovesCode()
        {
            var options = CreateOptions(nameof(CompleteAddressVerification_AddsVerifiedAddressAndRemovesCode));
            var factory = new TestDbContextFactory(options);

            var verificationCode = new VerificationCode
            {
                UserId = 7,
                AddressType = AddressType.Email,
                Address = "complete@example.com",
                VerificationCodeHash = "hash-to-remove",
                Expires = DateTime.UtcNow.AddHours(1),
                FailedAttempts = 0
            };

            await using (var seedContext = new ProfileDbContext(options))
            {
                seedContext.VerificationCodes.Add(verificationCode);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            await repository.CompleteAddressVerificationAsync(verificationCode.VerificationCodeId, AddressType.Email, "complete@example.com", 7);

            await using var assertContext = new ProfileDbContext(options);
            var verifiedAddress = await assertContext.VerifiedAddresses.FirstOrDefaultAsync(
                va => va.UserId == 7 && va.Address == "complete@example.com",
                cancellationToken: TestContext.Current.CancellationToken);
            var remainingCode = await assertContext.VerificationCodes.FirstOrDefaultAsync(
                vc => vc.UserId == 7 && vc.Address == "complete@example.com",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(verifiedAddress);
            Assert.Equal(AddressType.Email, verifiedAddress.AddressType);
            Assert.Null(remainingCode);
        }

        [Fact]
        public async Task CompleteAddressVerification_RemovesExistingVerificationsBeforeAdding()
        {
            var options = CreateOptions(nameof(CompleteAddressVerification_RemovesExistingVerificationsBeforeAdding));
            var factory = new TestDbContextFactory(options);

            var verificationCode = new VerificationCode
            {
                UserId = 8,
                AddressType = AddressType.Email,
                Address = "duplicate@example.com",
                VerificationCodeHash = "new-hash",
                Expires = DateTime.UtcNow.AddHours(1),
                FailedAttempts = 0
            };

            await using (var seedContext = new ProfileDbContext(options))
            {
                seedContext.VerificationCodes.Add(verificationCode);
                seedContext.VerifiedAddresses.Add(new VerifiedAddress
                {
                    UserId = 8,
                    AddressType = AddressType.Email,
                    Address = "duplicate@example.com",
                    VerificationType = VerificationType.Verified,
                });
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            await repository.CompleteAddressVerificationAsync(verificationCode.VerificationCodeId, AddressType.Email, "duplicate@example.com", 8);

            await using var assertContext = new ProfileDbContext(options);
            var verifiedAddresses = await assertContext.VerifiedAddresses
                .Where(va => va.UserId == 8 && va.Address == "duplicate@example.com")
                .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.Single(verifiedAddresses);
        }

        [Fact]
        public async Task IncrementFailedAttempts_IncrementsCounter()
        {
            var options = CreateOptions(nameof(IncrementFailedAttempts_IncrementsCounter));
            var factory = new TestDbContextFactory(options);

            var verificationCode = new VerificationCode
            {
                UserId = 10,
                AddressType = AddressType.Sms,
                Address = "+4799999998",
                VerificationCodeHash = "hash-fail",
                Expires = DateTime.UtcNow.AddHours(1),
                FailedAttempts = 2
            };

            await using (var seedContext = new ProfileDbContext(options))
            {
                seedContext.VerificationCodes.Add(verificationCode);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            await repository.IncrementFailedAttemptsAsync(verificationCode.VerificationCodeId);

            await using var assertContext = new ProfileDbContext(options);
            var updatedCode = await assertContext.VerificationCodes.FirstOrDefaultAsync(
                vc => vc.UserId == 10 && vc.Address == "+4799999998",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(updatedCode);
            Assert.Equal(3, updatedCode.FailedAttempts);
        }

        [Fact]
        public async Task IncrementFailedAttempts_StartsFromZero_IncrementsToOne()
        {
            var options = CreateOptions(nameof(IncrementFailedAttempts_StartsFromZero_IncrementsToOne));
            var factory = new TestDbContextFactory(options);

            var verificationCode = new VerificationCode
            {
                UserId = 11,
                AddressType = AddressType.Email,
                Address = "firstfail@example.com",
                VerificationCodeHash = "hash-first-fail",
                Expires = DateTime.UtcNow.AddHours(1),
                FailedAttempts = 0
            };

            await using (var seedContext = new ProfileDbContext(options))
            {
                seedContext.VerificationCodes.Add(verificationCode);
                await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var repository = new AddressVerificationRepository(factory);

            await repository.IncrementFailedAttemptsAsync(verificationCode.VerificationCodeId);

            await using var assertContext = new ProfileDbContext(options);
            var updatedCode = await assertContext.VerificationCodes.FirstOrDefaultAsync(
                vc => vc.UserId == 11 && vc.Address == "firstfail@example.com",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(updatedCode);
            Assert.Equal(1, updatedCode.FailedAttempts);
        }
    }
}
