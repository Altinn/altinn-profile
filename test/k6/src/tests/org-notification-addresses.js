import { check } from 'k6';
import * as config from '../config.js';
import { generateToken } from '../api/token-generator.js';
import * as orgNotificationAddressesApi from '../api/org-notification-addresses.js';
import { stopIterationOnFail } from "../errorhandler.js";

// Eksempel på bruk:
// podman compose run k6 run /src/tests/org-notification-addresses.js \
//   -e baseUrl=https://localhost:5000 \
//   -e tokenGeneratorUserName=*** \
//   -e tokenGeneratorUserPwd=*** \
//   -e tokenGeneratorUrl=https://localhost:5001/api/v1/token \
//   -e orgNo=***

export let options = {
    vus: 1,
    iterations: 1,
};

/**
 * Initialize test data.
 * @returns {Object} The data object containing token, runFullTestSet, sendersReference, and emailOrderRequest.
 */
export function setup() {
    const orgNo = __ENV.orgNo;
    const token = generateToken(config.tokenGenerator.getPersonalToken);

    const address = {
        phone: "99999994",
        countryCode: "+47"
    }

    const updateAddress = {
        email: "noreply1@altinn.no"
    }

    return {
        token,
        orgNo,
        address,
        updateAddress
    };
}

/**
 * Gets org notification addresses.
 * @param {Object} data - The data object containing token.
 */
function getOrgNotificationAddresses(data) {
    const response = orgNotificationAddressesApi.getOrgNotificationAddresses(
        data.token,
        data.orgNo
    );

    let success = check(response, {
        'GET org notification addresses: 200 OK': (r) => r.status === 200,
    });

    stopIterationOnFail("GET org notification addresses failed", success);
}

/**
 * Adds an org notification address.
 * @param {Object} data - The data object containing orgNo and token.
 * @returns {string} The selfLink of the created address.
 */
function addOrgNotificationAddresses(data) {
    const response = orgNotificationAddressesApi.addOrgNotificationAddresses(
        data.token,
        data.orgNo,
        data.address
    );

    let success = check(response, {
        'POST org notification addresses: 201 Created or 200 Ok': (r) => r.status === 201 || r.status === 200,
    });
    const selfLink = response.headers["Location"];

    stopIterationOnFail("POST org notification addresses failed", success);
    return selfLink;
}

/**
 * Updates an org notification address.
 * @param {Object} data - The data object containing orgNo and token.
 * @param {string} addressId - The id of the address to update.
 */
function updateOrgNotificationAddresses(data, addressId) {
    const response = orgNotificationAddressesApi.updateOrgNotificationAddresses(
        data.token,
        data.orgNo,
        data.updateAddress,
        addressId
    );

    let success = check(response, {
        'PUT org notification addresses: 200 Ok': (r) => r.status === 200,
    });

    stopIterationOnFail("PUT org notification addresses failed", success);
}

/**
 * Removes an org notification address.
 * @param {Object} data - The data object containing orgNo and token.
 * @param {string} addressId - The id of the address to update.
 */

function removeOrgNotificationAddresses(data, addressId) {
    const response = orgNotificationAddressesApi.removeOrgNotificationAddresses(
        data.token,
        data.orgNo,
        addressId
    );

    let success = check(response, {
        'DELETE org notification addresses: 200 Ok': (r) => r.status === 200,
    });

    stopIterationOnFail("DELETE org notification addresses failed", success);
}

/**
 * The main function to run the test.
 * @param {Object} data - The data object containing runFullTestSet and other test data.
 */
export default function (data) {
    getOrgNotificationAddresses(data);

    var link = addOrgNotificationAddresses(data);
    let addressId = link.split('/').pop();
    updateOrgNotificationAddresses(data, addressId);
    removeOrgNotificationAddresses(data, addressId);

}