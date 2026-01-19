import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as favoritesApi from '../api/favorites.js';
import { stopIterationOnFail } from "../errorhandler.js";
import { createCSVSharedArray, getRandomRow } from '../data/csv-loader.js';

// Eksempel på bruk:
// With environment variable (takes priority):
// podman compose run k6 run /src/tests/favorites.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//   -e partyUuid=***
//
// Without environment variable (uses CSV file with random row selection):
// podman compose run k6 run /src/tests/favorites.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=***

export const options = {
    vus: 1,
    iterations: 1,
    thresholds: {
        // Checks rate should be 100%. Raise error if any check has failed.
        checks: ['rate>=1']
    }
};
const csvData = createCSVSharedArray('favoritesTestData');

/**
 * Initialize test data.
 * Supports both CSV-based and environment variable-based test data.
 * Priority: Environment variables take precedence over CSV data.
 * - If partyUuid is provided: uses partyUuid from environment variables (user input takes priority)
 * - If partyUuid is not provided: loads static CSV file for random row selection
 * @returns {Object} The data object containing token and either csvData array or partyUuid.
 */
export function setup() {
    const partyUuid = __ENV.partyUuid;

    return {
        partyUuid,
    };
}

/**
 * Gets favorites.
 * @param {string} token - The authentication token.
 */
function getFavorites(token) {
    const response = favoritesApi.getFavorites(token);

    let success = check(response, {
        'GET favorites: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET favorites failed", success);
}

/**
 * Adds a favorite.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID to add as favorite.
 */
function addFavorites(token, partyUuid) {
    const response = favoritesApi.addFavorites(token, partyUuid);

    let success = check(response, {
        'PUT favorites: 201 Created or 204 No Content': (r) => r.status === 201 || r.status === 204,
    });

    stopIterationOnFail("PUT favorites failed", success);
}

/**
 * Removes a favorite.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID to remove from favorites.
 */
function removeFavorites(token, partyUuid) {
    const response = favoritesApi.removeFavorites(token, partyUuid);

    let success = check(response, {
        'DELETE favorites: 204 No Content': (r) => r.status === 204,
    });

    stopIterationOnFail("DELETE favorites failed", success);
}

/**
 * The main function to run the test.
 * Supports both CSV-based (random row selection) and environment variable-based approaches.
 * Priority: Environment variables take precedence over CSV data.
 * @param {Object} data - The data object containing csvData array (if using CSV) or partyUuid (if using env vars), and token.
 */
export default function runTests(data) {
    let partyUuid;
    let testRow = null;
    let useTestData = false;

    // Priority 1: Use environment variable if provided (user input takes precedence)
    if (data.partyUuid) {
        partyUuid = data.partyUuid;
    } else if (csvData && csvData.length > 0) {
        // Priority 2: Use CSV approach - select a random row from CSV data for this iteration
        testRow = getRandomRow(csvData);
        partyUuid = testRow.partyUuid;
        useTestData = true;
    } else {
        stopIterationOnFail("No test data available: neither partyUuid environment variable nor CSV data", false);
        return;
    }
    
    // Generate token for this iteration: environment variables take priority, CSV data used as fallback
    const token = generateToken(config.tokenGenerator.getPersonalToken, useTestData, testRow);
    
    addFavorites(token, partyUuid);
    getFavorites(token);
    removeFavorites(token, partyUuid);
}