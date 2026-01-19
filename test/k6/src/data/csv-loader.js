/**
 * CSV loader utility for k6 tests.
 * Loads CSV files and provides functions to randomly select rows for test iterations.
 * Uses SharedArray for efficient memory usage across all Virtual Users.
 */

import { SharedArray } from 'k6/data';

/**
 * Static CSV file path for test data.
 * This is the default CSV file used when environment variables are not provided.
 */
export const DEFAULT_CSV_DATA_FILE = '../data/orgs-in-yt01-with-party-uuid.csv';

/**
 * Parses a CSV file and returns an array of objects.
 * @param {string} csvContent - The CSV file content as a string.
 * @returns {Array<Object>} Array of objects where each object represents a row with column names as keys.
 */
function parseCSV(csvContent) {
    const lines = csvContent.trim().split('\n');
    if (lines.length < 2) {
        return [];
    }

    // Parse header row
    const headers = lines[0].split(',').map(h => h.trim());

    // Parse data rows
    const rows = [];
    for (let i = 1; i < lines.length; i++) {
        const values = lines[i].split(',').map(v => v.trim());
        const row = {};
        headers.forEach((header, index) => {
            row[header] = values[index] || '';
        });
        rows.push(row);
    }

    return rows;
}

/**
 * Creates a SharedArray for CSV data.
 * IMPORTANT: This must be called directly from setup() function, not from nested functions.
 * The SharedArray constructor must be in the init context.
 * @param {string} arrayName - Unique name for the SharedArray (used for caching).
 * @returns {SharedArray} SharedArray containing parsed CSV rows.
 */
export function createCSVSharedArray(arrayName = 'csvData') {
    const filePath = DEFAULT_CSV_DATA_FILE;
    // Create SharedArray directly - this must be called from setup() (init context)
    return new SharedArray(arrayName, function () {
        const csvContent = open(filePath);
        return parseCSV(csvContent);
    });
}

/**
 * Randomly selects a row from the CSV data array.
 * @param {Array<Object>} csvData - Array of CSV row objects.
 * @returns {Object} A randomly selected row object.
 */
export function getRandomRow(csvData) {
    if (!csvData || csvData.length === 0) {
        throw new Error('CSV data is empty or not loaded');
    }
    const randomIndex = Math.floor(Math.random() * csvData.length);
    return csvData[randomIndex];
}
