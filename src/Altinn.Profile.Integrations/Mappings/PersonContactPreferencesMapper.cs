using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Mappings;

/// <summary>
/// Provides mapping functionality from <see cref="Person"/> to <see cref="PersonContactPreferences"/>.
/// </summary>
public static class PersonContactPreferencesMapper
{
    /// <summary>
    /// Maps a <see cref="Person"/> entity to a <see cref="PersonContactPreferences"/> record.
    /// </summary>
    /// <param name="person">The <see cref="Person"/> entity to map from.</param>
    /// <returns>A <see cref="PersonContactPreferences"/> record containing mapped contact preferences.</returns>
    public static PersonContactPreferences Map(Person person)
    {
        return new PersonContactPreferences
        {
            Email = person.EmailAddress,
            IsReserved = person.Reservation ?? false,
            LanguageCode = person.LanguageCode,
            MobileNumber = person.MobilePhoneNumber,
            NationalIdentityNumber = person.FnumberAk
        };
    }
}
