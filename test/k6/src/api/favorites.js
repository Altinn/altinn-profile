import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getFavorites(token) {
  const endpoint = config.profileUrl.favorites;

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function addFavorites(token, partyUuid) {
    const endpoint = config.profileUrl.modifyFavorites(partyUuid);
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.put(endpoint, null, params);
  }

  export function removeFavorites(token, partyUuid) {
    const endpoint = config.profileUrl.modifyFavorites(partyUuid);
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
  
    return http.del(endpoint, null, params);
  }