import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getGroups(token) {
  const endpoint = config.profileUrl.groups;

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function getGroup(token, id) {
  const endpoint = config.profileUrl.groups + `/${id}`;

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function addGroup(token, groupName) {
    const endpoint = config.profileUrl.groups;

    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);

    return http.post(endpoint, JSON.stringify({ name: groupName }), params);
  }

  
export function renameGroup(token, id, groupName) {
    const endpoint = config.profileUrl.groups + `/${id}`;

    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);

    return http.patch(endpoint, JSON.stringify({ name: groupName }), params);
  }

export function addToGroup(token, id, partyUuid) {
    const endpoint = config.profileUrl.groups + `/${id}/associations/${partyUuid}`;

    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.put(endpoint, null, params);
  }

  export function removeFromGroup(token, id, partyUuid) {
    const endpoint = config.profileUrl.groups + `/${id}/associations/${partyUuid}`;

    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.del(endpoint, null, params);
  }

  export function deleteGroup(token, id) {
    const endpoint = config.profileUrl.groups + `/${id}`;

    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.del(endpoint, null, params);
  }