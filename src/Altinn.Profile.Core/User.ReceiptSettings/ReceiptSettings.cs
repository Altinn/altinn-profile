namespace Altinn.Profile.Core.User.ReceiptSettings;

/// <summary>
/// The receipt settings for a user.
/// </summary>
public class ReceiptSettings
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The UUID of the user.
    /// </summary>
    public Guid UserUuid { get; set; }

    /// <summary>
    /// The profile type for the user.
    /// </summary>
    public ProfileType ProfileType { get; set; }

    /// <summary>
    /// Whether the user har requested receipts for form submissions
    /// </summary>
    public bool? RequestReceipt { get; set; }
}
