import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as notificationSettingsApi from '../api/notificationsettings.js';
import { stopIterationOnFail } from "../errorhandler.js";
import { createCSVSharedArray, getRandomRow } from '../data/csv-loader.js';

// Eksempel på bruk:
// With environment variable (takes priority):
// podman compose run k6 run /src/tests/notification-settings.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//   -e partyUuid=***
//
// Without environment variable (uses CSV file with random row selection):
// podman compose run k6 run /src/tests/notification-settings.js \
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

const testData = createCSVSharedArray('notificationSettingsTestData');
// Access length to trigger initialization in init context
if (testData.length === 0) {
    stopIterationOnFail("No test data available: partyUuid not provided, and CSV file is empty", false);
}


/**
 * Initialize test data.
 * Supports both CSV-based and environment variable-based test data.
 * Priority: Environment variables take precedence over CSV data.
 * - If partyUuid is provided: uses partyUuid from environment variables (user input takes priority)
 * - If partyUuid is not provided: loads static CSV file for random row selection
 * @returns {Object} The data object containing token, csvData array or partyUuid, and notification settings.
 */
export function setup() {
    const partyUuid = __ENV.partyUuid;

    const notificationSettings = {
        emailAddress: "noreply-1@altinn.no",
        phoneNumber: "+4799999997",
        resourceIncludeList: ["urn:altinn:resource:example"]
    }

    return {
        partyUuid,
        notificationSettings
    };
}

/**
 * Gets notification settings.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID.
 */
function getPersonalNotificationAddresses(token, partyUuid) {
    const response = notificationSettingsApi.getPersonalNotificationAddresses(
        token,
        partyUuid
    );

    let success = check(response, {
        'GET notification settings: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET notification settings failed", success);
}

/**
 * Adds a notification setting.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID.
 * @param {Object} notificationSettings - The notification settings to add.
 */
function addPersonalNotificationAddresses(token, partyUuid, notificationSettings) {
    const response = notificationSettingsApi.addPersonalNotificationAddresses(
        token,
        partyUuid,
        notificationSettings
    );

    let success = check(response, {
        'PUT notification settings: 201 Created or 204 No Content': (r) => r.status === 201 || r.status === 204,
    });

    stopIterationOnFail("PUT notification settings failed", success);
}

/**
 * Removes a notification setting.
 * @param {string} token - The authentication token.
 * @param {string} partyUuid - The party UUID.
 */
function removePersonalNotificationAddresses(token, partyUuid) {
    const response = notificationSettingsApi.removePersonalNotificationAddresses(
        token,
        partyUuid
    );

    let success = check(response, {
        'DELETE notification settings: 200 Ok': (r) => r.status === 200,
    });

    stopIterationOnFail("DELETE notification settings failed", success);
}

/**
 * The main function to run the test.
 * Supports both CSV-based (random row selection) and environment variable-based approaches.
 * Priority: Environment variables take precedence over CSV data.
 * @param {Object} data - The data object containing partyUuid (if using env vars), and notificationSettings.
 */
export default function runTests(data) {
    let partyUuid;
    let testRow = null;

    // Priority 1: Use environment variable if provided (user input takes precedence)
    if (data.partyUuid) {
        partyUuid = data.partyUuid;
    } else if (testData && testData.length > 0) {
        // Priority 2: Use CSV approach - select a random row from CSV data for this iteration
        testRow = getRandomRow(testData);
        partyUuid = testRow.partyUuid;
    } else {
        stopIterationOnFail("No test data available: neither partyUuid environment variable nor CSV data", false);
        return;
    }
    
    // Generate token for this iteration: environment variables take priority, CSV data used as fallback
    const token = generateToken(config.tokenGenerator.getPersonalToken, testRow);
    
    addPersonalNotificationAddresses(token, partyUuid, data.notificationSettings);
    getPersonalNotificationAddresses(token, partyUuid);
    removePersonalNotificationAddresses(token, partyUuid);
}