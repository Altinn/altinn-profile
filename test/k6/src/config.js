import { stopIterationOnFail } from "./errorhandler.js";

// Base URLs for the Altinn platform across different environments.
const baseUrls = {
    prod: "altinn.no",
    tt02: "tt02.altinn.no",
    yt01: "yt01.altinn.cloud",
    at22: "at22.altinn.cloud",
    at23: "at23.altinn.cloud",
    at24: "at24.altinn.cloud"
};

const environment = __ENV.altinn_env ? __ENV.altinn_env.toLowerCase() : null;
if (!environment) {
    stopIterationOnFail("Environment variable 'altinn_env' is not set", false);
}

const baseUrl = baseUrls[environment];
if (!baseUrl) {
    stopIterationOnFail(`Invalid value for environment variable 'altinn_env': '${environment}'.`, false);
}

// Altinn TestTools token generator URL.
export const tokenGenerator = {
    getEnterpriseToken:
        "https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken",
    getPersonalToken:
        "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken"
};

export const profileUrl = {
    currentUser: `https://platform.${baseUrl}/profile/api/v1/users/current`,
    favorites: `https://platform.${baseUrl}/profile/api/v1/users/current/party-groups/favorites`,
    verification: `https://platform.${baseUrl}/profile/api/v1/users/current/verification`,
    modifyFavorites: (partyUuid) => `https://platform.${baseUrl}/profile/api/v1/users/current/party-groups/favorites/${partyUuid}`,
    organization: (orgNo) => `https://platform.${baseUrl}/profile/api/v1/organizations/${orgNo}/notificationaddresses/mandatory`,
    personalNotificationAddresses: (partyUuid) => `https://platform.${baseUrl}/profile/api/v1/users/current/notificationsettings/parties/${partyUuid}`,
}