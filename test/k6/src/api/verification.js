import http from "k6/http";
import * as config from "../config.js";
import * as apiHelpers from "../apiHelpers.js";

export function getVerifiedAddresses(token) {
  const endpoint = config.profileUrl.verification + '/verified-addresses';

  const params = apiHelpers.buildHeaderWithBearer(token);

  return http.get(endpoint, params);
}

export function verifyAddress(token, request) {
    const endpoint = config.profileUrl.verification + '/verify';
  
    const params = apiHelpers.buildHeaderWithBearerAndContentType(token);
    const requestBody = JSON.stringify(request);

    return http.post(endpoint, requestBody, params);
  }