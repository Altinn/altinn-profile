import http from "k6/http";
import encoding from "k6/encoding";
import secrets from "k6/secrets";
import * as apiHelpers from "../apiHelpers.js";
import { stopIterationOnFail, throwConfigurationError } from "../errorhandler.js";


const userID = __ENV.userID;
const pid = __ENV.pid;
const partyId = __ENV.partyId;
const environment = __ENV.altinn_env.toLowerCase();

/**
 * Generates a token by making an HTTP GET request to the specified Token endpoint.
 * Priority: Environment variables take precedence over CSV data.
 * Uses environment variables if provided, otherwise falls back to CSV test data.
 *
 * @param {string} endpoint - The endpoint URL to which the token generation request is sent.
 * @param {boolean} useTestData - Flag indicating whether to use test data from CSV.
 * @param {Object} testData - Optional test data object from CSV row. Should contain userId, ssn, and userPartyId.
 *                            Only used if environment variables (userID, pid, partyId) are not provided.
 * @returns {Promise<string>} The generated token.
 */
export async function generateToken(endpoint, useTestdata, testData = null) {
    const tokenGeneratorUserName = await getFromSecretSource('tokenGeneratorUserName', throwConfigurationError);
    const tokenGeneratorUserPwd = await getFromSecretSource('tokenGeneratorUserPwd', throwConfigurationError);


    const queryParams = {
        env: environment,
        scopes: "altinn:portal/enduser",
        userID: userID,
        pid: pid,
        partyId: partyId,
    };

    if (useTestdata) {
        queryParams.userID = testData.userId;
        queryParams.pid = testData.ssn;
        queryParams.partyId = testData.userPartyId;
    }


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

async function getFromSecretSource(secretName, raiseError) {
    let secretValue;
    try {
        secretValue = await secrets.get(secretName);
    }
    catch (error) {
        if (error == "no secret sources are configured") {
            raiseError("No secret source is configured for the k6 command - specify the file path with the --secret-source flag");
        }
        else if (error == "no value") {
            raiseError(`Secret ${secretName} does not exist in the secret source`);
        }
        raiseError("Unknown error occurred in the attempt to get secret from source");
    }
    if (!secretValue) {
        raiseError(`Secret ${secretName} is not properly assigned in the secret source`);
    }
    return secretValue;
}
