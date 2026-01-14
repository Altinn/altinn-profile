import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as userApi from '../api/user.js';
import { stopIterationOnFail } from "../errorhandler.js";
import { createCSVSharedArray, getRandomRow } from '../data/csv-loader.js';

// Eksempel på bruk:
// With environment variable (takes priority):
// podman compose run k6 run /src/tests/users.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//
// Without environment variable (uses CSV file with random row selection):
// podman compose run k6 run /src/tests/users.js \
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
const csvData = createCSVSharedArray('usersTestData');

if (csvData.length === 0) {
    stopIterationOnFail("No test data available: CSV file is empty", false);
}

/**
 * Initialize test data.
 */
export function setup() {
const userId = __ENV.userID;

    return {
        userId,
    };
}

/**
 * Gets a logged in user.
 * @param {string} token - The authentication token.
 */
function getUser(token) {
    const response = userApi.getUser(token);

    let success = check(response, {
        'GET favorites: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET favorites failed", success);
}

/**
 * The main function to run the test.
 * Supports both CSV-based (random row selection) and environment variable-based approaches.
 * Priority: Environment variables take precedence over CSV data.
 * @param {Object} data - The data object containing csvData array (if using CSV) or userId (if using env vars), and token.
 */
export default function runTests(data) {
    let testRow = null;
    let useTestData = false;

    // Priority 1: Use environment variable if provided (user input takes precedence)
    if (data.userId) {
        // Use userId from environment variable
    } else if (csvData && csvData.length > 0) {
        // Priority 2: Use CSV approach - select a random row from CSV data for this iteration
        testRow = getRandomRow(csvData);
        useTestData = true;
    } else {
        stopIterationOnFail("No test data available: neither partyUuid environment variable nor CSV data", false);
        return;
    }
    
    // Generate token for this iteration: environment variables take priority, CSV data used as fallback
    const token = generateToken(config.tokenGenerator.getPersonalToken, useTestData, testRow);
    
    getUser(token);
}