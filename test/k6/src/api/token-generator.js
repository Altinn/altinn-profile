import http from "k6/http";
import encoding from "k6/encoding";
import * as apiHelpers from "../apiHelpers.js";
import { stopIterationOnFail } from "../errorhandler.js";

const tokenGeneratorUserPwd = __ENV.tokenGeneratorUserPwd;
const tokenGeneratorUserName = __ENV.tokenGeneratorUserName;
const userID = __ENV.userID;
const pid = __ENV.pid;
const partyId = __ENV.partyId;
const environment = __ENV.altinn_env.toLowerCase();

/**
 * Generates a token by making an HTTP GET request to the specified Token endpoint.
 *
 * @param {string} endpoint - The endpoint URL to which the token generation request is sent.
 * @returns {string} The generated token.
 */
export function generateToken(endpoint) {
    if (!tokenGeneratorUserName) {
        stopIterationOnFail(`Invalid value for environment variable 'tokenGeneratorUserName': '${tokenGeneratorUserName}'.`, false);
    }

    if (!tokenGeneratorUserPwd) {
        stopIterationOnFail(`Invalid value for environment variable 'tokenGeneratorUserPwd': '${tokenGeneratorUserPwd}'.`, false);
    }
    const queryParams = {
        env: environment,
        userID: userID,
        pid: pid,
        partyId: partyId,
    };

    const endpointWithParams = endpoint + apiHelpers.buildQueryParametersForEndpoint(queryParams);

    const credentials = `${tokenGeneratorUserName}:${tokenGeneratorUserPwd}`;

    const encodedCredentials = encoding.b64encode(credentials);

    const params = apiHelpers.buildHeaderWithBasic(encodedCredentials);

    const response = http.get(endpointWithParams, params);

    if (response.status != 200) {
        stopIterationOnFail("Token generation failed", false);
    }

    const token = response.body;

    return token;
}
