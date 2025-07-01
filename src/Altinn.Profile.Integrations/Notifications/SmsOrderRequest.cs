namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Represents a request to order an SMS notification.
    /// </summary>
    internal class SmsOrderRequest
    {
        /// <summary>
        /// Reference from the sender for tracking or idempotency purposes.
        /// Example: "12345-notif"
        /// </summary>
        public string? SendersReference { get; set; }

        /// <summary>
        /// Unique idempotency identifier for the request.
        /// Used to prevent duplicate processing.
        /// Example: "12345"
        /// </summary>
        public required string IdempotencyId { get; set; }

        /// <summary>
        /// The recipient information for the SMS notification.
        /// </summary>
        public required Recipient Recipient { get; set; }
    }

    /// <summary>
    /// Contains recipient details for the SMS notification.
    /// </summary>
    internal class Recipient
    {
        /// <summary>
        /// SMS-specific recipient information.
        /// </summary>
        public required RecipientSms RecipientSms { get; set; }
    }

    /// <summary>
    /// Represents the SMS recipient and related settings.
    /// </summary>
    internal class RecipientSms
    {
        /// <summary>
        /// The recipient's phone number in international format.
        /// Example: "+4712345678"
        /// </summary>
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// SMS sending settings, such as time policy and message body.
        /// </summary>
        public required SmsSettings SmsSettings { get; set; }
    }

    /// <summary>
    /// Settings for sending the SMS, including timing and message content.
    /// </summary>
    internal class SmsSettings
    {
        /// <summary>
        /// Policy for when the SMS can be sent.
        /// Example: "Anytime"
        /// </summary>
        public string SendingTimePolicy { get; set; } = "Anytime";

        /// <summary>
        /// The body content of the SMS message.
        /// </summary>
        public required string Body { get; set; }
    }
}
