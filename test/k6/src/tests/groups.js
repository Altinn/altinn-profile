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
 * Helper to run a k6 check and stop iteration on failure.
 * Centralizes duplicated pattern: check(response, ...) + stopIterationOnFail(...)
 */
function checkAndStop(response, checksObj, failMessage) {
    const success = check(response, checksObj);
    stopIterationOnFail(failMessage, success);
    return success;
}

/**
 * Gets groups.
 * @param {string} token - The authentication token.
 */
function getGroups(token) {
    const response = groupsApi.getGroups(token);

    checkAndStop(response, { 'GET groups: 200 OK': (r) => r.status === 200 }, "GET groups failed");
}

/**
 * Creates a new group.
 * @param {string} token - The authentication token.
 * @param {string} name - The name of the group to create.
 */
function createGroup(token, name) {
    const response = groupsApi.addGroup(token, name);

    checkAndStop(response, { 'POST group: 201 Created': (r) => r.status === 201 }, "POST group failed");

    // Try to parse and return created group's id if present
    try {
        const body = JSON.parse(response.body || '{}');
        return body.id || body.groupId || null;
    } catch (e) {
        console.error('Failed to parse create group response body:', e);
        return null;
    }
}

/**
 * Renames an existing group.
 * @param {string} token - The authentication token.
 * @param {string} id - The id of the group to rename.
 * @param {string} newName - The new name for the group.
 */
function renameGroup(token, id, newName) {
    const response = groupsApi.renameGroup(token, id, newName);

    checkAndStop(response, { 'PATCH group: 200 OK or 204 No Content': (r) => r.status === 200 || r.status === 204 }, "PATCH group failed");
}

/**
 * Adds a party to a group (association).
 * @param {string} token - The authentication token.
 * @param {string} id - The id of the group.
 * @param {string} partyUuid - The party UUID to add.
 */
function addToGroup(token, id, partyUuid) {
    const response = groupsApi.addToGroup(token, id, partyUuid);

    checkAndStop(response, { 'PUT association: 200 OK': (r) => r.status === 200 }, "PUT association failed");
}

/**
 * Retrieves a single group by id.
 * @param {string} token - The authentication token.
 * @param {string} id - The id of the group to retrieve.
 */
function getGroup(token, id) {
    const response = groupsApi.getGroup(token, id);

    checkAndStop(response, { 'GET group: 200 OK': (r) => r.status === 200 }, "GET group failed");
}

/**
 * Removes a party from a group (association).
 * @param {string} token - The authentication token.
 * @param {string} id - The id of the group.
 * @param {string} partyUuid - The party UUID to remove.
 */
function removeFromGroup(token, id, partyUuid) {
    const response = groupsApi.removeFromGroup(token, id, partyUuid);

    checkAndStop(response, { 'DELETE association: 200 OK': (r) => r.status === 200 }, "DELETE association failed");
}

/**
 * Deletes a group by id.
 * @param {string} token - The authentication token.
 * @param {string} id - The id of the group to delete.
 */
function deleteGroup(token, id) {
    const response = groupsApi.deleteGroup(token, id);

    checkAndStop(response, { 'DELETE group: 204 No Content': (r) => r.status === 204 }, "DELETE group failed");
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

    const timestamp = new Date().toISOString();
    const baseName = `k6-test-group`;
    const groupName = `${baseName}-${timestamp}`;

    // 1. Create group
    const groupId = createGroup(token, groupName);

    if (groupId) {
            // 2. Rename group

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