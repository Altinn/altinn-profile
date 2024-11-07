namespace Altinn.Profile.Core.User.ContactPoints;

/// <summary>
/// Class describing the contact points for a user
/// </summary>
public class UserContactPoints
{
    private string _nationalIdentityNumber = string.Empty;
    private string _mobileNumber = string.Empty;
    private string _email = string.Empty;

    /// <summary>
    /// Gets or sets the national identityt number of the user
    /// </summary>
    public string NationalIdentityNumber
    {
        get => _nationalIdentityNumber;
        set
        { 
            if (value != null)
            {
                _nationalIdentityNumber = value;
            }
            else
            {
                _nationalIdentityNumber = string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets or sets the mobile number
    /// </summary>
    public string MobileNumber
    {
        get => _mobileNumber;
        set
        { 
            if (value != null)
            {
                _mobileNumber = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email
    {
        get => _email;
        set
        {
            if (value != null)
            {
                _email = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the ID of the user
    /// </summary>
    public int UserId { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets a boolean indicating whether the user has reserved themselves from electronic communication
    /// </summary>
    public bool IsReserved { get; set; }
}

/// <summary>
/// A list representation of <see cref="UserContactPoints"/>
/// </summary>
public class UserContactPointsList
{
    /// <summary>
    /// A list containing contact points for users
    /// </summary>
    public List<UserContactPoints> ContactPointsList { get; set; } = [];
}
