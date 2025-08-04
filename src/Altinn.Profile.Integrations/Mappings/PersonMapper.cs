using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// Provides mapping functionality from <see cref="PersonContactPreferencesSnapshot"/> to <see cref="Person"/>.
/// </summary>
public static class PersonMapper
{
    /// <summary>
    /// Maps a <see cref="PersonContactPreferencesSnapshot"/> to a <see cref="Person"/> entity.
    /// </summary>
    /// <param name="snapshot">The snapshot containing person contact preferences.</param>
    /// <returns>A <see cref="Person"/> entity populated with data from the snapshot.</returns>
    public static Person Map(PersonContactPreferencesSnapshot snapshot)
    {
        return new Person
        {
            LanguageCode = snapshot.Language,
            FnumberAk = snapshot.PersonIdentifier,
            Reservation = snapshot.Reservation == "JA",
            EmailAddress = GetContactDetail(snapshot, detail => detail.Email),
            MobilePhoneNumber = GetContactDetail(snapshot, detail => detail.MobileNumber),
            EmailAddressLastUpdated = GetContactDetailDate(snapshot, detail => detail.EmailLastUpdated),
            EmailAddressLastVerified = GetContactDetailDate(snapshot, detail => detail.EmailLastVerified),
            MobilePhoneNumberLastUpdated = GetContactDetailDate(snapshot, detail => detail.MobileNumberLastUpdated),
            MobilePhoneNumberLastVerified = GetContactDetailDate(snapshot, detail => detail.MobileNumberLastVerified)
        };
    }

    /// <summary>
    /// Gets a contact detail value from the snapshot using the provided selector.
    /// </summary>
    /// <param name="src">The snapshot containing contact details.</param>
    /// <param name="selector">A function to select the desired detail from <see cref="PersonContactDetailsSnapshot"/>.</param>
    /// <returns>The selected contact detail value, or null if not available.</returns>
    private static string? GetContactDetail(PersonContactPreferencesSnapshot src, Func<PersonContactDetailsSnapshot, string?> selector)
    {
        return src.ContactDetailsSnapshot != null ? selector(src.ContactDetailsSnapshot) : null;
    }

    /// <summary>
    /// Gets a contact detail date value from the snapshot using the provided selector and converts it to UTC.
    /// </summary>
    /// <param name="src">The snapshot containing contact details.</param>
    /// <param name="selector">A function to select the desired date from <see cref="PersonContactDetailsSnapshot"/>.</param>
    /// <returns>The selected contact detail date in UTC, or null if not available.</returns>
    private static DateTime? GetContactDetailDate(PersonContactPreferencesSnapshot src, Func<PersonContactDetailsSnapshot, DateTime?> selector)
    {
        if (src.ContactDetailsSnapshot != null)
        {
            var date = selector(src.ContactDetailsSnapshot);

            if (date.HasValue)
            {
                return date.Value.ToUniversalTime();
            }
        }

        return null;
    }
}
