using System.Collections.Generic;

using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Tests.Testdata;

/// <summary>
/// Provides test data for person contact and reservation information.
/// </summary>
public static class PersonTestData
{
    /// <summary>
    /// Gets a list of test registers with predefined contact and reservation data.
    /// </summary>
    /// <returns>A list of <see cref="Register"/> objects containing test data.</returns>
    public static List<Person> GetContactAndReservationTestData()
    {
        return
        [
            new()
            {
                LanguageCode = "nb",
                Reservation = false,
                FnumberAk = "17111933790",
                EmailAddress = "user1@example.com",
                MobilePhoneNumber = "+4790077853"
            },
            new()
            {
                LanguageCode = "nn",
                Reservation = false,
                FnumberAk = "06010941251",
                EmailAddress = "user2@example.com",
                MobilePhoneNumber = "+4790077854"
            },
            new()
            {
                LanguageCode = "en",
                Reservation = false,
                FnumberAk = "28026698350",
                EmailAddress = "user3@example.com",
                MobilePhoneNumber = "+4790077855"
            },
            new()
            {
                LanguageCode = "nb",
                Reservation = false,
                FnumberAk = "08117494927",
                EmailAddress = "user4@example.com",
                MobilePhoneNumber = "+4790077856"
            },
            new()
            {
                LanguageCode = "nn",
                Reservation = false,
                FnumberAk = "11044314101",
                EmailAddress = "user5@example.com",
                MobilePhoneNumber = "+4790077857"
            },
            new()
            {
                LanguageCode = "en",
                Reservation = false,
                FnumberAk = "07035704609",
                EmailAddress = "user6@example.com",
                MobilePhoneNumber = "+4790077858"
            },
            new()
            {
                LanguageCode = "nb",
                Reservation = false,
                FnumberAk = "24064316776",
                EmailAddress = "user7@example.com",
                MobilePhoneNumber = "+4790077859"
            },
            new()
            {
                LanguageCode = "nn",
                Reservation = false,
                FnumberAk = "20011400125",
                EmailAddress = "user8@example.com",
                MobilePhoneNumber = "+4790077860"
            },
            new()
            {
                LanguageCode = "en",
                Reservation = false,
                FnumberAk = "13049846538",
                EmailAddress = "user9@example.com",
                MobilePhoneNumber = "+4790077861"
            },
            new()
            {
                LanguageCode = "nb",
                Reservation = false,
                FnumberAk = "13045517963",
                EmailAddress = "user10@example.com",
                MobilePhoneNumber = "+4790077862"
            }
        ];
    }
}
