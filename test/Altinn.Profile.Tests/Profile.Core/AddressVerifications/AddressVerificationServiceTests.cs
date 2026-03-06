using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.AddressVerifications;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Telemetry;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.AddressVerifications;

public class AddressVerificationServiceTests : IDisposable
{
    private readonly Mock<IUserNotifier> _userNotifierMock = new();
    private readonly Mock<IAddressVerificationRepository> _repositoryMock = new();
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock = new();
    private readonly Telemetry _telemetry = new();
    private readonly MeterListener _meterListener;
    private readonly List<(string InstrumentName, double Value, KeyValuePair<string, object>[] Tags)> _recordedMeasurements = [];
    private readonly List<(string InstrumentName, long Value)> _recordedCounters = [];
    private readonly AddressVerificationService _sut;

    public AddressVerificationServiceTests()
    {
        var settings = Options.Create(new AddressMaintenanceSettings
        {
            VerificationCodeResendCooldownSeconds = 60
        });

        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == Telemetry.AppName)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        _meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            var tagList = new List<KeyValuePair<string, object>>();
            foreach (var tag in tags)
            {
                tagList.Add(new KeyValuePair<string, object>(tag.Key, tag.Value));
            }

            _recordedMeasurements.Add((instrument.Name, measurement, tagList.ToArray()));
        });

        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            _recordedCounters.Add((instrument.Name, measurement));
        });

        _meterListener.Start();

        _sut = new AddressVerificationService(
            _userNotifierMock.Object,
            _repositoryMock.Object,
            _verificationCodeServiceMock.Object,
            settings,
            _telemetry);
    }

    public void Dispose()
    {
        _meterListener?.Dispose();
        _telemetry?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForEmailAddress_ReturnsSuccess()
    {
        // Arrange
        ArrangeSuccessfulEmailResend(userId: 123, address: "test@example.com", codeCreatedMinutesAgo: 3);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(123, "test@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.Success, result);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForEmailAddress_RecordsPatienceMeasurement()
    {
        // Arrange
        ArrangeSuccessfulEmailResend(userId: 123, address: "test@example.com", codeCreatedMinutesAgo: 3);

        // Act
        await _sut.ResendVerificationCodeAsync(123, "test@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        var (_, value, tags) = Assert.Single(_recordedMeasurements, m => m.InstrumentName == "profile.verification.resend_patience_seconds");
        Assert.True(value >= 180, $"Expected at least 180 seconds waited, but got {value}");
        Assert.Contains(tags, t => t.Key == "address_type" && (string)t.Value == "Email");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForEmailAddress_DoesNotIncrementCodeNotFoundCounter()
    {
        // Arrange
        ArrangeSuccessfulEmailResend(userId: 100, address: "nocounters@example.com", codeCreatedMinutesAgo: 3);

        // Act
        await _sut.ResendVerificationCodeAsync(100, "nocounters@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForEmailAddress_DoesNotIncrementCooldownRejectedCounter()
    {
        // Arrange
        ArrangeSuccessfulEmailResend(userId: 100, address: "nocounters@example.com", codeCreatedMinutesAgo: 3);

        // Act
        await _sut.ResendVerificationCodeAsync(100, "nocounters@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForPhoneAddress_ReturnsSuccess()
    {
        // Arrange
        ArrangeSuccessfulSmsResend(userId: 321, address: "12345678", codeCreatedMinutesAgo: 2);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(321, "12345678", AddressType.Sms, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.Success, result);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenResendingForPhoneAddress_RecordsPatienceMeasurementWithSmsTag()
    {
        // Arrange
        ArrangeSuccessfulSmsResend(userId: 321, address: "12345678", codeCreatedMinutesAgo: 2);

        // Act
        await _sut.ResendVerificationCodeAsync(321, "12345678", AddressType.Sms, CancellationToken.None);

        // Assert
        var (_, value, tags) = Assert.Single(_recordedMeasurements, m => m.InstrumentName == "profile.verification.resend_patience_seconds");
        Assert.True(value >= 120, $"Expected at least 120 seconds waited, but got {value}");
        Assert.Contains(tags, t => t.Key == "address_type" && (string)t.Value == "Sms");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_ReturnsCodeCooldown()
    {
        // Arrange
        ArrangeCooldownScenario(userId: 456, address: "user@example.com", codeCreatedSecondsAgo: 30);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(456, "user@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeCooldown, result);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_RecordsCooldownRejectedCounter()
    {
        // Arrange
        ArrangeCooldownScenario(userId: 456, address: "user@example.com", codeCreatedSecondsAgo: 30);

        // Act
        await _sut.ResendVerificationCodeAsync(456, "user@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Contains(_recordedCounters, c => c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_RecordsPatienceMeasurement()
    {
        // Arrange
        ArrangeCooldownScenario(userId: 456, address: "user@example.com", codeCreatedSecondsAgo: 30);

        // Act
        await _sut.ResendVerificationCodeAsync(456, "user@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Single(_recordedMeasurements, m => m.InstrumentName == "profile.verification.resend_patience_seconds");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_DoesNotIncrementCodeNotFoundCounter()
    {
        // Arrange
        ArrangeCooldownScenario(userId: 300, address: "onlycooldown@example.com", codeCreatedSecondsAgo: 10);

        // Act
        await _sut.ResendVerificationCodeAsync(300, "onlycooldown@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_ReturnsCodeNotFound()
    {
        // Arrange
        ArrangeNoExistingCode(userId: 789, address: "nocode@example.com");

        // Act
        var result = await _sut.ResendVerificationCodeAsync(789, "nocode@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeNotFound, result);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_RecordsCodeNotFoundCounter()
    {
        // Arrange
        ArrangeNoExistingCode(userId: 789, address: "nocode@example.com");

        // Act
        await _sut.ResendVerificationCodeAsync(789, "nocode@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Contains(_recordedCounters, c => c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_DoesNotRecordPatienceMeasurement()
    {
        // Arrange
        ArrangeNoExistingCode(userId: 789, address: "nocode@example.com");

        // Act
        await _sut.ResendVerificationCodeAsync(789, "nocode@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Empty(_recordedMeasurements);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_DoesNotIncrementCooldownRejectedCounter()
    {
        // Arrange
        ArrangeNoExistingCode(userId: 200, address: "onlynotfound@example.com");

        // Act
        await _sut.ResendVerificationCodeAsync(200, "onlynotfound@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenConcurrentInsertFails_ReturnsCodeCooldown()
    {
        // Arrange
        ArrangeConcurrentInsertFailure(userId: 999, address: "concurrent@example.com", codeCreatedMinutesAgo: 5);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(999, "concurrent@example.com", AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeCooldown, result);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenConcurrentInsertFails_StillRecordsPatienceMeasurement()
    {
        // Arrange
        ArrangeConcurrentInsertFailure(userId: 999, address: "concurrent@example.com", codeCreatedMinutesAgo: 5);

        // Act
        await _sut.ResendVerificationCodeAsync(999, "concurrent@example.com", AddressType.Email, CancellationToken.None);

        // Assert — telemetry is recorded before the insert attempt, so it should still be present
        var measurement = Assert.Single(_recordedMeasurements, m => m.InstrumentName == "profile.verification.resend_patience_seconds");
        Assert.Contains(measurement.Tags, t => t.Key == "address_type" && (string)t.Value == "Email");
    }

    // ── Arrange helpers ─────────────────────────────────────────────────
    private void ArrangeSuccessfulEmailResend(int userId, string address, int codeCreatedMinutesAgo)
    {
        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-codeCreatedMinutesAgo),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        var newCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "newhash",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock.Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>())).ReturnsAsync(existingCode);
        _verificationCodeServiceMock.Setup(s => s.GenerateRawCode()).Returns("654321");
        _verificationCodeServiceMock.Setup(s => s.CreateVerificationCode(userId, address, AddressType.Email, "654321")).Returns(newCode);
        _repositoryMock.Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>())).ReturnsAsync(true);
        _userNotifierMock.Setup(n => n.SendVerificationCodeAsync(userId, address, AddressType.Email, "654321", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private void ArrangeSuccessfulSmsResend(int userId, string address, int codeCreatedMinutesAgo)
    {
        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Sms,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-codeCreatedMinutesAgo),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        var newCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Sms,
            Address = address,
            VerificationCodeHash = "newhash",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock.Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Sms, address, It.IsAny<CancellationToken>())).ReturnsAsync(existingCode);
        _verificationCodeServiceMock.Setup(s => s.GenerateRawCode()).Returns("111222");
        _verificationCodeServiceMock.Setup(s => s.CreateVerificationCode(userId, address, AddressType.Sms, "111222")).Returns(newCode);
        _repositoryMock.Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>())).ReturnsAsync(true);
        _userNotifierMock.Setup(n => n.SendVerificationCodeAsync(userId, address, AddressType.Sms, "111222", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private void ArrangeCooldownScenario(int userId, string address, int codeCreatedSecondsAgo)
    {
        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddSeconds(-codeCreatedSecondsAgo),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock.Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>())).ReturnsAsync(existingCode);
    }

    private void ArrangeNoExistingCode(int userId, string address)
    {
        _repositoryMock.Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>())).ReturnsAsync((VerificationCode)null);
    }

    private void ArrangeConcurrentInsertFailure(int userId, string address, int codeCreatedMinutesAgo)
    {
        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-codeCreatedMinutesAgo),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        var newCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "newhash",
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock.Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>())).ReturnsAsync(existingCode);
        _verificationCodeServiceMock.Setup(s => s.GenerateRawCode()).Returns("999888");
        _verificationCodeServiceMock.Setup(s => s.CreateVerificationCode(userId, address, AddressType.Email, "999888")).Returns(newCode);
        _repositoryMock.Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>())).ReturnsAsync(false);
    }
}
