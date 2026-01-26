namespace Altinn.Profile.Integrations.Notifications
{
    /// <summary>
    /// Represents a request to order an email notification.
    /// </summary>
    internal class EmailOrderRequest
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
        /// The recipient information for the email notification.
        /// </summary>
        public required RecipientEmail RecipientEmail { get; set; }
    }

    /// <summary>
    /// Represents the email recipient and related settings.
    /// </summary>
    internal class RecipientEmail
    {
        /// <summary>
        /// The recipient's email address.
        /// Example: "user@example.com"
        /// </summary>
        public required string EmailAddress { get; set; }

        /// <summary>
        /// Email sending settings, such as subject, body, content type, and time policy.
        /// </summary>
        public required EmailSettings EmailSettings { get; set; }
    }

    /// <summary>
    /// Settings for sending the email, including subject, body, content type, and timing.
    /// </summary>
    internal class EmailSettings
    {
        /// <summary>
        /// The subject of the email.
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// The body content of the email message.
        /// </summary>
        public required string Body { get; set; }

        /// <summary>
        /// The content type of the email body (e.g., "Plain" or "Html").
        /// </summary>
        public string ContentType { get; set; } = "Html";

        /// <summary>
        /// Policy for when the email can be sent.
        /// Example: "Anytime"
        /// </summary>
        public string SendingTimePolicy { get; set; } = "Anytime";
    }
}
