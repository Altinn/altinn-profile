import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getOrgNotificationAddresses(token, orgNo) {
  const endpoint = config.profileUrl.organization(orgNo);

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function addOrgNotificationAddresses(token, orgNo, address) {
    const endpoint = config.profileUrl.organization(orgNo);
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    const requestBody = JSON.stringify(address);
    return http.post(endpoint, requestBody, params);
  }

  export function updateOrgNotificationAddresses(token, orgNo, address, addressId) {
    const endpoint = config.profileUrl.organization(orgNo)+'/'+addressId;
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
    const requestBody = JSON.stringify(address);
    return http.put(endpoint, requestBody, params);
  }

  export function removeOrgNotificationAddresses(token, orgNo, addressId) {
    const endpoint = config.profileUrl.organization(orgNo)+'/'+addressId;
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.del(endpoint, null, params);
  }