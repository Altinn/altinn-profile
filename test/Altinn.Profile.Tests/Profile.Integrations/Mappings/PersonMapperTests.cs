using System;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Mappings;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Mappings
{
    public class PersonMapperTests
    {
        [Fact]
        public void Map_MapsSnapshotToPerson_AllFieldsMappedCorrectly()
        {
            // Arrange
            var now = DateTime.Now;
            var details = new PersonContactDetailsSnapshot
            {
                Email = "test@example.com",
                EmailLastVerified = now,
                EmailLastUpdated = now.AddDays(-1),
                IsEmailDuplicated = null,
                IsMobileNumberDuplicated = null,
                MobileNumber = "12345678",
                MobileNumberLastVerified = now.AddHours(-2),
                MobileNumberLastUpdated = now.AddHours(-3)
            };

            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = details,
                Language = "NO",
                LanguageLastUpdated = null,
                NotificationStatus = null,
                PersonIdentifier = "12345678901",
                Reservation = "JA",
                Status = null
            };

            // Act
            var person = PersonMapper.Map(snapshot);

            // Assert
            Assert.Equal("NO", person.LanguageCode);
            Assert.Equal("12345678901", person.FnumberAk);
            Assert.True(person.Reservation);
            Assert.Equal("test@example.com", person.EmailAddress);
            Assert.Equal("12345678", person.MobilePhoneNumber);
            Assert.Equal(now.AddDays(-1).ToUniversalTime(), person.EmailAddressLastUpdated);
            Assert.Equal(now.ToUniversalTime(), person.EmailAddressLastVerified);
            Assert.Equal(now.AddHours(-3).ToUniversalTime(), person.MobilePhoneNumberLastUpdated);
            Assert.Equal(now.AddHours(-2).ToUniversalTime(), person.MobilePhoneNumberLastVerified);
        }

        [Fact]
        public void Map_HandlesNullContactDetailsSnapshot_ContactFieldsAreNull()
        {
            // Arrange
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = null,
                Language = "EN",
                LanguageLastUpdated = null,
                NotificationStatus = null,
                PersonIdentifier = "98765432109",
                Reservation = "NEI",
                Status = null
            };

            // Act
            var person = PersonMapper.Map(snapshot);

            // Assert
            Assert.Equal("EN", person.LanguageCode);
            Assert.Equal("98765432109", person.FnumberAk);
            Assert.False(person.Reservation);
            Assert.Null(person.EmailAddress);
            Assert.Null(person.MobilePhoneNumber);
            Assert.Null(person.EmailAddressLastUpdated);
            Assert.Null(person.EmailAddressLastVerified);
            Assert.Null(person.MobilePhoneNumberLastUpdated);
            Assert.Null(person.MobilePhoneNumberLastVerified);
        }

        [Fact]
        public void GetContactDetail_ReturnsSelectedValue_WhenContactDetailsSnapshotIsNotNull()
        {
            // Arrange
            var details = new PersonContactDetailsSnapshot
            {
                Email = "foo@bar.com"
            };
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = details
            };

            // Act
            var result = typeof(PersonMapper)
                .GetMethod("GetContactDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { snapshot, new Func<PersonContactDetailsSnapshot, string?>(d => d.Email) });

            // Assert
            Assert.Equal("foo@bar.com", result);
        }

        [Fact]
        public void GetContactDetail_ReturnsNull_WhenContactDetailsSnapshotIsNull()
        {
            // Arrange
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = null
            };

            // Act
            var result = typeof(PersonMapper)
                .GetMethod("GetContactDetail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { snapshot, new Func<PersonContactDetailsSnapshot, string?>(d => d.Email) });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetContactDetailDate_ReturnsUtcDate_WhenDateIsPresent()
        {
            // Arrange
            var date = new DateTime(2024, 8, 4, 12, 0, 0, DateTimeKind.Local);
            var details = new PersonContactDetailsSnapshot
            {
                EmailLastVerified = date
            };
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = details
            };

            // Act
            var result = typeof(PersonMapper)
                .GetMethod("GetContactDetailDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { snapshot, new Func<PersonContactDetailsSnapshot, DateTime?>(d => d.EmailLastVerified) });

            // Assert
            Assert.Equal(date.ToUniversalTime(), result);
        }

        [Fact]
        public void GetContactDetailDate_ReturnsNull_WhenContactDetailsSnapshotIsNull()
        {
            // Arrange
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = null
            };

            // Act
            var result = typeof(PersonMapper)
                .GetMethod("GetContactDetailDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { snapshot, new Func<PersonContactDetailsSnapshot, DateTime?>(d => d.EmailLastVerified) });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetContactDetailDate_ReturnsNull_WhenDateIsNull()
        {
            // Arrange
            var details = new PersonContactDetailsSnapshot
            {
                EmailLastVerified = null
            };
            var snapshot = new PersonContactPreferencesSnapshot
            {
                ContactDetailsSnapshot = details
            };

            // Act
            var result = typeof(PersonMapper)
                .GetMethod("GetContactDetailDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { snapshot, new Func<PersonContactDetailsSnapshot, DateTime?>(d => d.EmailLastVerified) });

            // Assert
            Assert.Null(result);
        }
    }
}
