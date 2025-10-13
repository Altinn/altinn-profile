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
                ""doNotPromptForParty"": 0,
                ""preselectedPartyUuid"": ""8491ed2a-7716-4df1-ac18-bbfc0334f79d"",
                ""showClientUnits"": 0,
                ""shouldShowSubEntities"": 1,
                ""shouldShowDeletedEntities"": 0,
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
    }
}
