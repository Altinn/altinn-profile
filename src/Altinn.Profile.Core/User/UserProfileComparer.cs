using Altinn.Profile.Models;

using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Compares user profiles and logs mismatch metadata without logging sensitive values.
/// </summary>
public sealed class UserProfileComparer : IUserProfileComparer
{
    private readonly ILogger<UserProfileComparer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileComparer"/> class.
    /// </summary>
    /// <param name="logger">Logger used for mismatch diagnostics.</param>
    public UserProfileComparer(ILogger<UserProfileComparer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<UserProfileMismatch> CompareAndLog(UserProfile? source, UserProfile? target)
    {
        List<UserProfileMismatch> mismatches = new();

        CompareUserProfile(source, target, mismatches);

        int userId = source?.UserId ?? target?.UserId ?? 0;
        Models.Enums.UserType userType = source?.UserType ?? target?.UserType ?? Models.Enums.UserType.None;

        foreach (UserProfileMismatch mismatch in mismatches)
        {
            _logger.LogWarning(
                "User profile shadow mismatch detected for userId {UserId} and userType {UserType}. Field: {FieldPath}. MismatchType: {MismatchType}.",
                userId,
                userType,
                mismatch.FieldPath,
                mismatch.MismatchType);
        }

        return mismatches;
    }

    private static void CompareUserProfile(UserProfile? source, UserProfile? target, ICollection<UserProfileMismatch> mismatches)
    {
        if (!CompareObjectPresence("UserProfile", source, target, mismatches))
        {
            return;
        }

        CompareField("UserId", source!.UserId, target!.UserId, mismatches);
        CompareField("UserUuid", source.UserUuid, target.UserUuid, mismatches);
        CompareField("UserName", source.UserName, target.UserName, mismatches);
        CompareField("ExternalIdentity", source.ExternalIdentity, target.ExternalIdentity, mismatches);
        CompareField("IsReserved", source.IsReserved, target.IsReserved, mismatches);
        CompareField("PartyId", source.PartyId, target.PartyId, mismatches);
        CompareField("UserType", source.UserType, target.UserType, mismatches);

        CompareParty(source.Party, target.Party, mismatches);
    }

    private static void CompareParty(Register.Contracts.V1.Party? source, Register.Contracts.V1.Party? target, ICollection<UserProfileMismatch> mismatches)
    {
        if (!CompareObjectPresence("Party", source, target, mismatches))
        {
            return;
        }

        CompareField("Party.PartyId", source!.PartyId, target!.PartyId, mismatches);
        CompareField("Party.PartyUuid", source.PartyUuid, target.PartyUuid, mismatches);
        CompareField("Party.PartyTypeName", source.PartyTypeName, target.PartyTypeName, mismatches);
        CompareField("Party.OrgNumber", source.OrgNumber, target.OrgNumber, mismatches);
        CompareField("Party.SSN", source.SSN, target.SSN, mismatches);
        CompareField("Party.Name", source.Name, target.Name, mismatches);
        CompareField("Party.IsDeleted", source.IsDeleted, target.IsDeleted, mismatches);

        ComparePerson(source.Person, target.Person, mismatches);
    }

    private static void ComparePerson(Register.Contracts.V1.Person? source, Register.Contracts.V1.Person? target, ICollection<UserProfileMismatch> mismatches)
    {
        if (!CompareObjectPresence("Party.Person", source, target, mismatches))
        {
            return;
        }

        CompareField("Party.Person.SSN", source!.SSN, target!.SSN, mismatches);
        CompareField("Party.Person.Name", source.Name, target.Name, mismatches);
        CompareField("Party.Person.FirstName", source.FirstName, target.FirstName, mismatches);
        CompareField("Party.Person.MiddleName", source.MiddleName, target.MiddleName, mismatches);
        CompareField("Party.Person.LastName", source.LastName, target.LastName, mismatches);
        CompareField("Party.Person.TelephoneNumber", source.TelephoneNumber, target.TelephoneNumber, mismatches);
        CompareField("Party.Person.MailingAddress", source.MailingAddress, target.MailingAddress, mismatches);
        CompareField("Party.Person.MailingPostalCode", source.MailingPostalCode, target.MailingPostalCode, mismatches);
        CompareField("Party.Person.MailingPostalCity", source.MailingPostalCity, target.MailingPostalCity, mismatches);
        CompareField("Party.Person.AddressMunicipalNumber", source.AddressMunicipalNumber, target.AddressMunicipalNumber, mismatches);
        CompareField("Party.Person.AddressMunicipalName", source.AddressMunicipalName, target.AddressMunicipalName, mismatches);
        CompareField("Party.Person.AddressStreetName", source.AddressStreetName, target.AddressStreetName, mismatches);
        CompareField("Party.Person.AddressHouseNumber", source.AddressHouseNumber, target.AddressHouseNumber, mismatches);
        CompareField("Party.Person.AddressHouseLetter", source.AddressHouseLetter, target.AddressHouseLetter, mismatches);
        CompareField("Party.Person.AddressPostalCode", source.AddressPostalCode, target.AddressPostalCode, mismatches);
        CompareField("Party.Person.AddressCity", source.AddressCity, target.AddressCity, mismatches);
        CompareField("Party.Person.DateOfDeath", source.DateOfDeath, target.DateOfDeath, mismatches);
    }

    private static bool CompareObjectPresence(string fieldPath, object? source, object? target, ICollection<UserProfileMismatch> mismatches)
    {
        if (source == null && target == null)
        {
            return false;
        }

        if (source != null && target == null)
        {
            mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.NotFoundInRegister));
            return false;
        }

        if (source == null)
        {
            mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.MissingField));
            return false;
        }

        return true;
    }

    private static void CompareField(string fieldPath, object? source, object? target, ICollection<UserProfileMismatch> mismatches)
    {
        if (source is null && target is null)
        {
            return;
        }

        if ((source is null && target is string rightString && rightString.Length == 0)
            || (target is null && source is string leftString && leftString.Length == 0))
        {
            mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.NullVsEmptyString));
            return;
        }

        if (source is null || target is null)
        {
            mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.MissingField));
            return;
        }

        if (source is string left && target is string right)
        {
            if (!string.Equals(left, right, StringComparison.Ordinal))
            {
                if (string.Equals(NormalizeWhitespace(left), NormalizeWhitespace(right), StringComparison.Ordinal))
                {
                    mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.ExtraSpaces));
                    return;
                }

                mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.WrongValue));
            }

            return;
        }

        if (!Equals(source, target))
        {
            mismatches.Add(new UserProfileMismatch(fieldPath, UserProfileMismatchType.WrongValue));
        }
    }

    private static string NormalizeWhitespace(string value)
    {
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// The mismatch category detected when comparing two user profiles.
    /// </summary>
    public enum UserProfileMismatchType
    {
        /// <summary>
        /// One side is missing a value while the other side has a value.
        /// </summary>
        MissingField,

        /// <summary>
        /// One side is <see langword="null"/> while the other side is an empty string.
        /// </summary>
        NullVsEmptyString,

        /// <summary>
        /// Both sides have values, but one side has extra or repeated spaces.
        /// </summary>
        ExtraSpaces,

        /// <summary>
        /// Both sides have values, but the values are different.
        /// </summary>
        WrongValue,

        /// <summary>
        /// The userProfile could not be found in register
        /// </summary>
        NotFoundInRegister,

    }

    /// <summary>
    /// Describes a single mismatch between two user profiles.
    /// </summary>
    /// <param name="FieldPath">The logical field path where the mismatch was found.</param>
    /// <param name="MismatchType">The detected mismatch type.</param>
    public readonly record struct UserProfileMismatch(string FieldPath, UserProfileMismatchType MismatchType);
}
