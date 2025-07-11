import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getPersonalNotificationAddresses(token, partyUuid) {
  const endpoint = config.profileUrl.personalNotificationAddresses(partyUuid);

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function addPersonalNotificationAddresses(token, partyUuid, notificationSettings) {
    const endpoint = config.profileUrl.personalNotificationAddresses(partyUuid);
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
    const requestBody = JSON.stringify(notificationSettings);
    return http.put(endpoint, requestBody, params);
  }

  export function removePersonalNotificationAddresses(token, partyUuid) {
    const endpoint = config.profileUrl.personalNotificationAddresses(partyUuid);
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.del(endpoint, null, params);
  }