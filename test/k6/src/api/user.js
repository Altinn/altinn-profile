import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getUser(token) {
  const endpoint = config.profileUrl.currentUser;

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}
