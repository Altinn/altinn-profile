using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Service for handling operations related to user registration and contact points.
/// </summary>
public class RegisterService : IRegisterService
{
    private readonly IRegisterRepository _registerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterService"/> class.
    /// </summary>
    /// <param name="registerRepository">The repository used for accessing register data.</param>
    public RegisterService(IRegisterRepository registerRepository)
    {
        _registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
    }

    /// <summary>
    /// Asynchronously retrieves the contact point information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's register information, or <c>null</c> if not found.
    /// </returns>
    public async Task<IUserContactInfo?> GetUserContactInfoAsync(string nationalIdentityNumber)
    {
        if (!IsValidNationalIdentityNumber(nationalIdentityNumber))
        {
            return null;
        }

        var userContactInfo = await _registerRepository.GetUserContactInfoAsync(nationalIdentityNumber);
        return MapToUserContactInfo(userContactInfo);
    }

    /// <summary>
    /// Asynchronously retrieves the contact point information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of user register information, or an empty collection if none are found.
    /// </returns>
    public async Task<IEnumerable<IUserContactInfo>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        if (nationalIdentityNumbers == null || !nationalIdentityNumbers.Any())
        {
            return [];
        }

        if (!nationalIdentityNumbers.All(IsValidNationalIdentityNumber))
        {
            return [];
        }

        var userContactInfo = await _registerRepository.GetUserContactInfoAsync(nationalIdentityNumbers);
        return MapToUserContactInfo(userContactInfo);
    }

    /// <summary>
    /// Maps a <see cref="Register"/> entity to a <see cref="UserContactInfo"/> object.
    /// </summary>
    /// <param name="userContactInfo">The <see cref="Register"/> entity containing user contact information.</param>
    /// <returns>
    /// A <see cref="UserContactInfo"/> object containing the mapped contact information, or <c>null</c> if the input is <c>null</c>.
    /// </returns>
    private static UserContactInfo? MapToUserContactInfo(Register? userContactInfo)
    {
        if (userContactInfo is null)
        {
            return null;
        }

        return new UserContactInfo()
        {
            IsReserved = userContactInfo.Reservation,
            EmailAddress = userContactInfo.EmailAddress,
            LanguageCode = userContactInfo.LanguageCode,
            NationalIdentityNumber = userContactInfo.FnumberAk,
            MobilePhoneNumber = userContactInfo.MobilePhoneNumber,
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="Register"/> entities to a collection of <see cref="UserContactInfo"/> objects.
    /// </summary>
    /// <param name="userContactInfos">The collection of <see cref="Register"/> entities containing user contact information.</param>
    /// <returns>
    /// A collection of <see cref="UserContactInfo"/> objects containing the mapped contact information, or an empty collection if the input is <c>null</c>.
    /// </returns>
    private static IEnumerable<UserContactInfo> MapToUserContactInfo(IEnumerable<Register>? userContactInfos)
    {
        if (userContactInfos is null)
        {
            return [];
        }

        return userContactInfos.Select(userContactInfo => new UserContactInfo()
        {
            IsReserved = userContactInfo.Reservation,
            EmailAddress = userContactInfo.EmailAddress,
            LanguageCode = userContactInfo.LanguageCode,
            NationalIdentityNumber = userContactInfo.FnumberAk,
            MobilePhoneNumber = userContactInfo.MobilePhoneNumber,
        });
    }

    /// <summary>
    /// Validates the national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number to validate.</param>
    /// <returns>True if the national identity number is valid; otherwise, false.</returns>
    private bool IsValidNationalIdentityNumber(string? nationalIdentityNumber)
    {
        return !string.IsNullOrWhiteSpace(nationalIdentityNumber) && nationalIdentityNumber.IsDigitsOnly();
    }
}
