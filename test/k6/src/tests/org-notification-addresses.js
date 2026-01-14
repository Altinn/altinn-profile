import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as orgNotificationAddressesApi from '../api/org-notification-addresses.js';
import { stopIterationOnFail } from "../errorhandler.js";
import { createCSVSharedArray, getRandomRow } from '../data/csv-loader.js';

// Eksempel på bruk:
// With environment variable (takes priority):
// podman compose run k6 run /src/tests/org-notification-addresses.js \
//   -e altinn_env=*** \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e userID=*** \
//   -e partyId=*** \
//   -e pid=*** \
//   -e orgNo=***
//
// Without environment variable (uses CSV file with random row selection):
// podman compose run k6 run /src/tests/org-notification-addresses.js \
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

const csvData = createCSVSharedArray('orgNotificationAddressesTestData');
// Access length to trigger initialization in init context
if (csvData.length === 0) {
    stopIterationOnFail("No test data available: orgNo not provided, and CSV file is empty", false);
}

/**
 * Initialize test data.
 * Supports both CSV-based and environment variable-based test data.
 * Priority: Environment variables take precedence over CSV data.
 * - If orgNo is provided: uses orgNo from environment variables (user input takes priority)
 * - If orgNo is not provided: loads static CSV file for random row selection
 * @returns {Object} The data object containing token, csvData array or orgNo, and address settings.
 */
export function setup() {
    const orgNo = __ENV.orgNo;
    
   
    const envSuffix = __ENV.altinn_env.slice(-1);
    const numericSuffix = Number.parseInt(envSuffix);
    const suffix = Number.isInteger(numericSuffix) ? envSuffix : 0;

    const address = {
        phone: "9999999" + suffix,
        countryCode: "+47"
    }
    const updateAddress = {
        email: "noreply" + suffix + "@altinn.no"
    }

    return {
        orgNo,
        address,
        updateAddress
    };
}

/**
 * Gets org notification addresses.
 * @param {string} token - The authentication token.
 * @param {string} orgNo - The organization number.
 */
function getOrgNotificationAddresses(token, orgNo) {
    const response = orgNotificationAddressesApi.getOrgNotificationAddresses(
        token,
        orgNo
    );

    let success = check(response, {
        'GET org notification addresses: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET org notification addresses failed", success);
}

/**
 * Adds an org notification address.
 * @param {string} token - The authentication token.
 * @param {string} orgNo - The organization number.
 * @param {Object} address - The address object to add.
 * @returns {string} The id of the created address.
 */
function addOrgNotificationAddresses(token, orgNo, address) {
    const response = orgNotificationAddressesApi.addOrgNotificationAddresses(
        token,
        orgNo,
        address
    );

    let success = check(response, {
        'POST org notification addresses: 201 Created or 200 Ok': (r) => r.status === 201 || r.status === 200,
    });

    if (!success) {
        console.error(`POST org notification addresses failed with status ${response.status}: ${response.body}`);
    }

    stopIterationOnFail("POST org notification addresses failed", success);
    const id = JSON.parse(response.body).notificationAddressId;

    return id;
}

/**
 * Updates an org notification address.
 * @param {string} token - The authentication token.
 * @param {string} orgNo - The organization number.
 * @param {Object} updateAddress - The address object to update.
 * @param {string} addressId - The id of the address to update.
 * @returns {string} The id of the created address.
 */
function updateOrgNotificationAddresses(token, orgNo, updateAddress, addressId) {
    const response = orgNotificationAddressesApi.updateOrgNotificationAddresses(
        token,
        orgNo,
        updateAddress,
        addressId
    );

    let success = check(response, {
        'PUT org notification addresses: 200 Ok or 409 Conflict': (r) => r.status === 200 || r.status === 409,
    });

    if (response.status === 409) {
        return addressId;
    }

    if (!success) {
        console.error(`PUT org notification addresses failed with status ${response.status}: ${response.body}`);
    }

    stopIterationOnFail("PUT org notification addresses failed", success);
    const id = JSON.parse(response.body).notificationAddressId;

    return id;
}

/**
 * Removes an org notification address.
 * @param {string} token - The authentication token.
 * @param {string} orgNo - The organization number.
 * @param {string} addressId - The id of the address to remove.
 */
function removeOrgNotificationAddresses(token, orgNo, addressId) {
    const response = orgNotificationAddressesApi.removeOrgNotificationAddresses(
        token,
        orgNo,
        addressId
    );

    let success = check(response, {
        'DELETE org notification addresses: 200 Ok or 409 Conflict': (r) => r.status === 200 || r.status === 409,
    });

    if (!success) {
        console.error(`DELETE org notification addresses failed with status ${response.status}: ${response.body}`);
    }

    stopIterationOnFail("DELETE org notification addresses failed", success);
}

/**
 * The main function to run the test.
 * Supports both CSV-based (random row selection) and environment variable-based approaches.
 * Priority: Environment variables take precedence over CSV data.
 * @param {Object} data - The data object containing csvData array (if using CSV) or orgNo (if using env vars), token, address, and updateAddress.
 */
export default function runTests(data) {
    let orgNo;
    let testRow = null;
    
    // Priority 1: Use environment variable if provided (user input takes precedence)
    if (data.orgNo) {
        orgNo = data.orgNo;
    } else if (csvData && csvData.length > 0) {
        // Priority 2: Use CSV approach - select a random row from CSV data for this iteration
        testRow = getRandomRow(csvData);
        orgNo = testRow.orgNo;
    } else {
        stopIterationOnFail("No test data available: neither orgNo environment variable nor CSV data", false);
        return;
    }
    
    // Generate token for this iteration: environment variables take priority, CSV data used as fallback
    const token = generateToken(config.tokenGenerator.getPersonalToken, testRow);

    let addressId = addOrgNotificationAddresses(token, orgNo, data.address);
    getOrgNotificationAddresses(token, orgNo);

    addressId = updateOrgNotificationAddresses(token, orgNo, data.updateAddress, addressId);
    removeOrgNotificationAddresses(token, orgNo, addressId);
}