using System.Diagnostics.Metrics;

using Altinn.Profile.Core.AddressVerifications.Models;

using static Altinn.Profile.Core.Telemetry.Telemetry.AddressVerification;

namespace Altinn.Profile.Core.Telemetry;

/// <summary>
/// Telemetry for address verification.
/// </summary>
partial class Telemetry
{
    private Histogram<double> _resendPatienceHistogram = null!;

    private void InitAddressVerification(InitContext context)
    {
        InitMetricCounter(context, _verificationResendCooldownRejectedCounterName, init: static m => m.Add(0));
        InitMetricCounter(context, _verificationResendCodeNotFoundCounterName, init: static m => m.Add(0));
        _resendPatienceHistogram = Meter.CreateHistogram<double>(
            _verificationResendPatienceSecondsName,
            unit: "s",
            description: "Time in seconds between verification code creation and user resend request");
    }

    /// <summary>
    /// Increments the counter for the number of verification codes not found when resending.
    /// </summary>
    public void RecordVerificationResendCodeNotFound(AddressType addressType) => _counters[_verificationResendCodeNotFoundCounterName].Add(1, AddressTypeTag(addressType));

    /// <summary>
    /// Increments the counter for the number of resend requests rejected due to cooldown.
    /// </summary>
    public void RecordVerificationResendCooldownRejected(AddressType addressType) => _counters[_verificationResendCooldownRejectedCounterName].Add(1, AddressTypeTag(addressType));

    /// <summary>
    /// Records the time (in seconds) a user waited before requesting a new verification code.
    /// </summary>
    /// <param name="secondsWaited">The number of seconds between code creation and resend request.</param>
    /// <param name="addressType">The type of address (e.g., Email, Sms).</param>
    public void RecordResendPatience(double secondsWaited, AddressType addressType)
    {
        _resendPatienceHistogram.Record(secondsWaited, AddressTypeTag(addressType));
    }

    private static KeyValuePair<string, object?> AddressTypeTag(AddressType addressType) => new("address_type", addressType.ToString());

    /// <summary>
    /// Constants for address verification telemetry metrics.
    /// </summary>
    internal static class AddressVerification
    {
        /// <summary>
        /// The metric name for the histogram tracking user resend patience (seconds waited before requesting a new code).
        /// </summary>
        internal static readonly string _verificationResendPatienceSecondsName = MetricName("resend_patience_seconds");

        /// <summary>
        /// The name of the metric for the number of rejected resends due to cooldown/timeout.
        /// </summary>
        internal static readonly string _verificationResendCooldownRejectedCounterName = MetricName("resend_cooldown-rejected");

        /// <summary>
        /// The name of the metric for the number of failed code lookups for the given user, address type, and address value.
        /// </summary>
        internal static readonly string _verificationResendCodeNotFoundCounterName = MetricName("resend_code-not-found");

        private static string MetricName(string name) => Metrics.CreateName($"verification.{name}");
    }
}
