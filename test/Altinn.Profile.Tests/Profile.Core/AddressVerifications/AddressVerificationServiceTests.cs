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
    private readonly List<(double Value, KeyValuePair<string, object>[] Tags)> _recordedMeasurements = [];
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

            _recordedMeasurements.Add((measurement, tagList.ToArray()));
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
    public async Task ResendVerificationCodeAsync_WhenCodeIsResent_RecordsResendPatienceTelemetry()
    {
        // Arrange
        const int userId = 123;
        const string address = "test@example.com";
        var codeCreatedTime = DateTime.UtcNow.AddMinutes(-3);

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = codeCreatedTime,
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

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        _verificationCodeServiceMock
            .Setup(s => s.GenerateRawCode())
            .Returns("654321");

        _verificationCodeServiceMock
            .Setup(s => s.CreateVerificationCode(userId, address, AddressType.Email, "654321"))
            .Returns(newCode);

        _repositoryMock
            .Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync(true);

        _userNotifierMock
            .Setup(n => n.SendVerificationCodeAsync(userId, address, AddressType.Email, "654321", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.Success, result);
        Assert.Single(_recordedMeasurements);

        var (value, tags) = _recordedMeasurements[0];
        Assert.True(value >= 180, $"Expected at least 180 seconds waited, but got {value}");
        Assert.Contains(tags, t => t.Key == "address_type" && (string)t.Value == "Email");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_RecordsCooldownRejectedAndSkipsHistogram()
    {
        // Arrange
        const int userId = 456;
        const string address = "user@example.com";

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddSeconds(-30), // Within the 60-second cooldown
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeCooldown, result);
        Assert.Empty(_recordedMeasurements);
        Assert.Contains(_recordedCounters, c => c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_RecordsCodeNotFoundAndSkipsHistogram()
    {
        // Arrange
        const int userId = 789;
        const string address = "nocode@example.com";

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VerificationCode)null);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeNotFound, result);
        Assert.Empty(_recordedMeasurements);
        Assert.Contains(_recordedCounters, c => c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenSmsResend_RecordsTelemetryWithSmsAddressType()
    {
        // Arrange
        const int userId = 321;
        const string address = "12345678";

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Sms,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-2),
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

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Sms, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        _verificationCodeServiceMock
            .Setup(s => s.GenerateRawCode())
            .Returns("111222");

        _verificationCodeServiceMock
            .Setup(s => s.CreateVerificationCode(userId, address, AddressType.Sms, "111222"))
            .Returns(newCode);

        _repositoryMock
            .Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync(true);

        _userNotifierMock
            .Setup(n => n.SendVerificationCodeAsync(userId, address, AddressType.Sms, "111222", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Sms, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.Success, result);
        Assert.Single(_recordedMeasurements);

        var (value, tags) = _recordedMeasurements[0];
        Assert.True(value >= 120, $"Expected at least 120 seconds waited, but got {value}");
        Assert.Contains(tags, t => t.Key == "address_type" && (string)t.Value == "Sms");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenConcurrentInsertFails_StillRecordsTelemetry()
    {
        // Arrange — the code is past cooldown but AddNewVerificationCodeAsync returns false (concurrent insert)
        const int userId = 999;
        const string address = "concurrent@example.com";

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-5),
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

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        _verificationCodeServiceMock
            .Setup(s => s.GenerateRawCode())
            .Returns("999888");

        _verificationCodeServiceMock
            .Setup(s => s.CreateVerificationCode(userId, address, AddressType.Email, "999888"))
            .Returns(newCode);

        _repositoryMock
            .Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync(false); // Concurrent insert

        // Act
        var result = await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert
        Assert.Equal(ResendVerificationResult.CodeCooldown, result);

        // Telemetry is recorded before the insert attempt, so it should still be present
        Assert.Single(_recordedMeasurements);
        Assert.Contains(_recordedMeasurements[0].Tags, t => t.Key == "address_type" && (string)t.Value == "Email");
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsResent_DoesNotIncrementCounters()
    {
        // Arrange
        const int userId = 100;
        const string address = "nocounters@example.com";

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddMinutes(-3),
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

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        _verificationCodeServiceMock
            .Setup(s => s.GenerateRawCode())
            .Returns("123456");

        _verificationCodeServiceMock
            .Setup(s => s.CreateVerificationCode(userId, address, AddressType.Email, "123456"))
            .Returns(newCode);

        _repositoryMock
            .Setup(r => r.AddNewVerificationCodeAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync(true);

        _userNotifierMock
            .Setup(n => n.SendVerificationCodeAsync(userId, address, AddressType.Email, "123456", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert — success path should not increment either error counter
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenNoExistingCode_DoesNotIncrementCooldownCounter()
    {
        // Arrange
        const int userId = 200;
        const string address = "onlynotfound@example.com";

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VerificationCode)null);

        // Act
        await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert — code-not-found incremented, but cooldown-rejected should NOT be
        Assert.Contains(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
    }

    [Fact]
    public async Task ResendVerificationCodeAsync_WhenCodeIsInCooldown_DoesNotIncrementCodeNotFoundCounter()
    {
        // Arrange
        const int userId = 300;
        const string address = "onlycooldown@example.com";

        var existingCode = new VerificationCode
        {
            UserId = userId,
            AddressType = AddressType.Email,
            Address = address,
            VerificationCodeHash = "somehash",
            Created = DateTime.UtcNow.AddSeconds(-10),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        _repositoryMock
            .Setup(r => r.GetVerificationCodeAsync(userId, AddressType.Email, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCode);

        // Act
        await _sut.ResendVerificationCodeAsync(userId, address, AddressType.Email, CancellationToken.None);

        // Assert — cooldown-rejected incremented, but code-not-found should NOT be
        Assert.Contains(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_cooldown-rejected_counter" && c.Value == 1);
        Assert.DoesNotContain(_recordedCounters, c =>
            c.InstrumentName == "profile.verification.resend_code-not-found_counter" && c.Value == 1);
    }
}
