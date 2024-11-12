using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Tests.Profile.Integrations.Extensions
{
    /// <summary>
    /// Extensions to help testing with the Person class 
    /// </summary>
    internal static class PersonExtensions
    {
        /// <summary>
        /// Custom mapper from Person -> PersonContactPreferences 
        /// </summary>
        internal static PersonContactPreferences AsPersonContactPreferences(this Person person)
        {
            return new PersonContactPreferences()
            {
                NationalIdentityNumber = person.FnumberAk,
                Email = person.EmailAddress,
                IsReserved = person.Reservation ?? false,
                LanguageCode = person.LanguageCode,
                MobileNumber = person.MobilePhoneNumber,
            };
        }
    }
}
