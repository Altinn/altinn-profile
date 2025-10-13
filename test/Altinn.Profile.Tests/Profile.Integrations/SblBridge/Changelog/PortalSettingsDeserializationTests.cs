using Altinn.Profile.Integrations.SblBridge.Changelog;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.Changelog
{
    public class PortalSettingsDeserializationTests
    {
        [Fact]
        public void Deserialize_ValidJson_ReturnsExpectedProfileSettings()
        {
            // Arrange
            var json = @"{
                ""userId"": 20000018,
                ""languageType"": 1044,
                ""doNotPromptForParty"": 0,
                ""preselectedPartyUuid"": """",
                ""showClientUnits"": 0,
                ""shouldShowSubEntities"": 1,
                ""shouldShowDeletedEntities"": 0,
                ""ignoreUnitProfileDateTime"": """"
            }";

            // Act
            var result = ChangeLogItem.PortalSettings.Deserialize(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20000018, result.UserId);
            Assert.Equal(1044, result.LanguageType);
            Assert.False(result.DoNotPromptForParty);
            Assert.Null(result.PreselectedPartyUuid);
            Assert.False(result.ShowClientUnits);
            Assert.True(result.ShouldShowSubEntities);
            Assert.False(result.ShouldShowDeletedEntities);
            Assert.Null(result.IgnoreUnitProfileDateTime);
        }

        [Fact]
        public void Deserialize_ValidJsonWithPreselectedPartyUuid_ReturnsExpectedProfileSettings()
        {
            // Arrange
            var json = @"{
                ""userId"": 20000018,
                ""languageType"": 1044,
                ""doNotPromptForParty"": false,
                ""preselectedPartyUuid"": ""8491ed2a-7716-4df1-ac18-bbfc0334f79d"",
                ""showClientUnits"": false,
                ""shouldShowSubEntities"": true,
                ""shouldShowDeletedEntities"": false,
                ""ignoreUnitProfileDateTime"": null
            }";

            // Act
            var result = ChangeLogItem.PortalSettings.Deserialize(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20000018, result.UserId);
            Assert.Equal(1044, result.LanguageType);
            Assert.False(result.DoNotPromptForParty);
            Assert.NotNull(result.PreselectedPartyUuid);
            Assert.False(result.ShowClientUnits);
            Assert.True(result.ShouldShowSubEntities);
            Assert.False(result.ShouldShowDeletedEntities);
            Assert.Null(result.IgnoreUnitProfileDateTime);
        }

        [Theory]
        [InlineData("2022-03-04 12:21:17.610")]
        [InlineData("2022-03-04T00:00:00")]
        [InlineData("2025-10-10T12:11:44.4596855+02:00")]
        public void Deserialize_ValidJsonWithIgnoreUnitProfileDateTime_ReturnsExpectedProfileSettings(string dateTime)
        {
            // Arrange
            var json = $@"{{
                ""userId"": 20000018,
                ""languageType"": 1044,
                ""doNotPromptForParty"": 0,
                ""preselectedPartyUuid"": """",
                ""showClientUnits"": 0,
                ""shouldShowSubEntities"": 1,
                ""shouldShowDeletedEntities"": 0,
                ""ignoreUnitProfileDateTime"": ""{dateTime}""
            }}";

            // Act
            var result = ChangeLogItem.PortalSettings.Deserialize(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20000018, result.UserId);
            Assert.Equal(1044, result.LanguageType);
            Assert.False(result.DoNotPromptForParty);
            Assert.Null(result.PreselectedPartyUuid);
            Assert.False(result.ShowClientUnits);
            Assert.True(result.ShouldShowSubEntities);
            Assert.False(result.ShouldShowDeletedEntities);
            Assert.NotNull(result.IgnoreUnitProfileDateTime);
        }
    }
}
