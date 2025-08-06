import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as notificationSettingsApi from '../api/notificationsettings.js';
import { stopIterationOnFail } from "../errorhandler.js";

// Eksempel på bruk:
// podman compose run k6 run /src/tests/notification-settings.js \
//   -e env=*** \
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

    const notificationSettings = {
        emailAddress: "noreply@altinn.no",
        phoneNumber: "+4799999999",
        resourceIncludeList: [ "urn:altinn:resource:example" ]
    }

    return {
        token,
        partyUuid,
        notificationSettings
    };
}

/**
 * Gets notification settings.
 * @param {Object} data - The data object containing token.
 */
function getPersonalNotificationAddresses(data) {
    const response = notificationSettingsApi.getPersonalNotificationAddresses(
        data.token,
        data.partyUuid
    );

    let success = check(response, {
        'GET notification settings: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET notification settings failed", success);
}

/**
 * Adds a notification setting.
 * @param {Object} data - The data object containing partyUuid and token.
 */
function addPersonalNotificationAddresses(data) {
    const response = notificationSettingsApi.addPersonalNotificationAddresses(
        data.token,
        data.partyUuid,
        data.notificationSettings
    );

    let success = check(response, {
        'PUT notification settings: 201 Created or 204 No Content': (r) => r.status === 201 || r.status === 204,
    });

    stopIterationOnFail("PUT notification settings failed", success);
}

/**
 * Removes a notification setting.
 * @param {Object} data - The data object containing partyUuid and token.
 */
function removePersonalNotificationAddresses(data) {
    const response = notificationSettingsApi.removePersonalNotificationAddresses(
        data.token,
        data.partyUuid
    );

    let success = check(response, {
        'DELETE notification settings: 200 Ok': (r) => r.status === 200,
    });

    stopIterationOnFail("DELETE notification settings failed", success);
}

/**
 * The main function to run the test.
 * @param {Object} data - The data object containing runFullTestSet and other test data.
 */
export default function (data) {
    addPersonalNotificationAddresses(data);
    getPersonalNotificationAddresses(data);
    removePersonalNotificationAddresses(data);
}