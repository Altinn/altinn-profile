import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as groupsApi from '../api/groups.js';
import { stopIterationOnFail } from "../errorhandler.js";
import { createCSVSharedArray, getRandomRow } from '../data/csv-loader.js';

// Eksempel på bruk:
// With environment variable (takes priority):
// podman compose run k6 run /src/tests/groups.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//   -e partyUuid=***
//
// Without environment variable (uses CSV file with random row selection):
// podman compose run k6 run /src/tests/groups.js \
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
const csvData = createCSVSharedArray('groupsTestData');

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
 * Gets groups.
 * @param {string} token - The authentication token.
 */
function getGroups(token) {
    const response = groupsApi.getGroups(token);

    let success = check(response, {
        'GET groups: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET groups failed", success);
}

/**
 * Adds a favorite.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID to add as favorite.
 */
function createGroup(token, name) {
    const response = groupsApi.addGroup(token, name);

    let success = check(response, {
        'POST group: 201 Created': (r) => r.status === 201,
    });

    stopIterationOnFail("POST group failed", success);

    // Try to parse and return created group's id if present
    try {
        const body = JSON.parse(response.body || '{}');
        return body.id || body.groupId || null;
    } catch (e) {
        console.error('Failed to parse create group response body:', e);
        return null;
    }
}

function renameGroup(token, id, newName) {
    const response = groupsApi.renameGroup(token, id, newName);

    let success = check(response, {
        'PATCH group: 200 OK or 204 No Content': (r) => r.status === 200 || r.status === 204,
    });

    stopIterationOnFail("PATCH group failed", success);
}

function addToGroup(token, id, partyUuid) {
    const response = groupsApi.addToGroup(token, id, partyUuid);

    let success = check(response, {
        'PUT association: 200 Created': (r) => r.status === 200
    });

    stopIterationOnFail("PUT association failed", success);
}

/**
 * Removes a favorite.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID to remove from groups.
 */
function getGroup(token, id) {
    const response = groupsApi.getGroup(token, id);

    let success = check(response, {
        'GET group: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET group failed", success);
}

function removeFromGroup(token, id, partyUuid) {
    const response = groupsApi.removeFromGroup(token, id, partyUuid);

    let success = check(response, {
        'DELETE association: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("DELETE association failed", success);
}

function deleteGroup(token, id) {
    const response = groupsApi.deleteGroup(token, id);

    let success = check(response, {
        'DELETE group: 204 No Content': (r) => r.status === 204,
    });

    stopIterationOnFail("DELETE group failed", success);
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
    // Generate unique group name to avoid collisions when running multiple times
    const timestamp = new Date().toISOString();
    const baseName = useTestData && testRow.groupName ? testRow.groupName : `k6-test-group`;
    const groupName = `${baseName}-${timestamp}`;

    // 1. Create group
    const createdId = createGroup(token, groupName);

    // If API didn't return id, try fetching groups and infer id from name
    let groupId = createdId;
    if (!groupId) {
        const listResp = groupsApi.getGroups(token);
        try {
            const listBody = JSON.parse(listResp.body || '[]');
            const found = (listBody || []).find((g) => g.name === groupName);
            groupId = found && (found.id || found.groupId) ? (found.id || found.groupId) : null;
        } catch (e) {
            console.error('Failed to parse groups list response body:', e);
            groupId = null;
        }
    }

    // 2. Rename group
    if (groupId) {
        renameGroup(token, groupId, `${groupName}-renamed`);

        // 3. Add association
        addToGroup(token, groupId, partyUuid);

        // 4. Get single group
        getGroup(token, groupId);

        // 5. Get groups list
        getGroups(token);

        // 6. Remove association
        removeFromGroup(token, groupId, partyUuid);

        // 7. Delete group
        deleteGroup(token, groupId);
    } else {
        stopIterationOnFail('Could not determine group id for further operations', false);
    }
}