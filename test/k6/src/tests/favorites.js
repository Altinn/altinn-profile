import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as favoritesApi from '../api/favorites.js';
import { stopIterationOnFail } from "../errorhandler.js";

// Eksempel på bruk:
// podman compose run k6 run /src/tests/favorites.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//   -e partyUuid=***

export const options = {
    vus: 1,
    iterations: 1,
    thresholds: {
        // Checks rate should be 100%. Raise error if any check has failed.
        checks: ['rate>=1']
    }
};

/**
 * Initialize test data.
 * @returns {Object} The data object containing token, runFullTestSet, sendersReference, and emailOrderRequest.
 */
export function setup() {
    const partyUuid = __ENV.partyUuid;
    const token = generateToken(config.tokenGenerator.getPersonalToken);

    return {
        token,
        partyUuid,
    };
}

/**
 * Gets favorites.
 * @param {Object} data - The data object containing token.
 */
function getFavorites(data) {
    const response = favoritesApi.getFavorites(
        data.token
    );

    let success = check(response, {
        'GET favorites: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET favorites failed", success);
}

/**
 * Adds a favorite.
 * @param {Object} data - The data object containing partyUuid and token.
 */
function addFavorites(data) {
    const response = favoritesApi.addFavorites(
        data.token,
        data.partyUuid
    );

    let success = check(response, {
        'PUT favorites: 201 Created or 204 No Content': (r) => r.status === 201 || r.status === 204,
    });

    stopIterationOnFail("PUT favorites failed", success);
}

/**
 * Removes a favorite.
 * @param {Object} data - The data object containing partyUuid and token.
 */
function removeFavorites(data) {
    const response = favoritesApi.removeFavorites(
        data.token,
        data.partyUuid
    );

    let success = check(response, {
        'DELETE favorites: 204 No Content': (r) => r.status === 204,
    });

    stopIterationOnFail("DELETE favorites failed", success);
}

/**
 * The main function to run the test.
 * @param {Object} data - The data object containing runFullTestSet and other test data.
 */
export default function runTests(data) {
    addFavorites(data);
    getFavorites(data);
    removeFavorites(data);
}