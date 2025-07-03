// Base URLs for the Altinn platform across different environments.
const baseUrls = {
    prod: "altinn.no",
    tt02: "tt02.altinn.no",
    yt01: "yt01.altinn.cloud",
    at22: "at22.altinn.cloud",
    at23: "at23.altinn.cloud",
    at24: "at24.altinn.cloud"
};

const environment = __ENV.env ? __ENV.env.toLowerCase() : null;
if (!environment) {
    stopIterationOnFail("Environment variable 'env' is not set", false);
}

const baseUrl = baseUrls[environment];
if (!baseUrl) {
    stopIterationOnFail(`Invalid value for environment variable 'env': '${environment}'.`, false);
}

// Altinn TestTools token generator URL.
export const tokenGenerator = {
    getEnterpriseToken:
        "https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken",
    getPersonalToken:
        "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken"
};

export const profileUrl = {
    favorites: `https://platform.${baseUrl}/profile/api/v1/users/current/party-groups/favorites`,
    modifyFavorites: (partyUuid) => `https://platform.${baseUrl}/profile/api/v1/users/current/party-groups/favorites/${partyUuid}`,
    organization: (orgNo) => `https://platform.${baseUrl}/profile/api/v1/organizations/${orgNo}/notificationaddresses/mandatory`,
    personalNotificationaddresses: (partyId) => `https://platform.${baseUrl}/profile/api/v1/users/current/notificationsettings/parties//${partyId}`,
}