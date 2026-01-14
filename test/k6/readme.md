# Automated tests using k6

## Install prerequisites

*We recommend running the tests through a Docker container.*

From the command line:

```bash
docker pull grafana/k6
```

Further information on [installing k6 for running in Docker is available here.](https://k6.io/docs/get-started/installation/#docker)

Alternatively, it is possible to run the tests directly on your machine as well.

[General installation instructions are available here.](https://k6.io/docs/get-started/installation/)

---

## Environment Variables

The following environment variables are required to run the tests:

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `altinn_env` | Environment to run tests against (prod/tt02/yt01/at22/at23/at24) | Yes | - |
| `tokenGeneratorUserName` | Username for token generator | Yes | - |
| `tokenGeneratorUserPwd` | Password for token generator | Yes | - |
| `partyUuid` | Party UUID for testing (for favorites and notification-settings tests) | No* | - |
| `orgNo` | Organization number for testing (for org-notification-addresses test) | No* | - |

\* If not provided, tests will use the static CSV file (`/src/data/orgs-in-yt01-with-party-uuid.csv`) with random row selection.

### Test Data Input Methods

Tests support **two methods** for providing test data. **Environment variables take priority over CSV data** - if both are provided, the environment variable will be used.

#### Method 1: Environment Variables (Takes Priority)

When environment variables are provided (e.g., `partyUuid` or `orgNo`), they take precedence over CSV data. This allows users to override CSV data with specific test values.

The CSV file should contain columns matching the test requirements:
- `orgNo` - Organization number
- `partyId` - Party ID
- `partyUuid` - Party UUID
- `userId` - User ID
- `ssn` - Social security number
- Other columns as needed by specific tests

Example CSV format:
```csv
orgNo,partyId,ssn,userId,userPartyId,orgUuid,orgType,partyUuid
730077254,58881276,10121251049,1292822,54077221,27229997-f13d-4f25-9416-748695fb8e22,regnskapsforer,2fa5d6a6-40dc-4f45-94d6-ec3a8de92224
```

#### Method 2: CSV-based (Recommended for Load Testing)

Each test iteration randomly selects a row from the CSV file, making it ideal for load and performance testing with diverse test data. CSV data is only used when environment variables are not provided.

Additional configuration options:

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `REQUEST_TIMEOUT` | Request timeout in seconds | No | 30s |
| `RATE_LIMIT` | Rate limit for requests | No | 100 |
| `RETRY_COUNT` | Number of retry attempts | No | 3 |
| `RETRY_INTERVAL` | Interval between retries (ms) | No | 1000 |

---

## Running tests

All tests are defined in the `src/tests` folder. At the top of each test file, an example command to run the test is provided.

> **Note: Command syntax for different shells**
> - **Bash**: Use the command as written below.
> - **PowerShell**: Replace `\` with a backtick (`` ` ``) at the end of each line.
> - **Command Prompt (cmd.exe)**: Replace `\` with `^` at the end of each line.

The command should be run from the `k6` folder:

```bash
$> cd /altinn-profile/test/k6
```

### Basic Test Execution

#### Using environment variables (takes priority):

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=yt01 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e partyUuid=***
```

This uses the provided `partyUuid` directly for all iterations.

#### Using static CSV file (automatic when no environment variables provided):

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=yt01 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=***
```

Each test iteration will randomly select a row from the static CSV file (`/src/data/orgs-in-yt01-with-party-uuid.csv`), ensuring diverse test data across iterations. This happens automatically when environment variables are not provided.

### Advanced Configuration

Run with custom timeout and retry settings (using static CSV file):

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=tt02 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e REQUEST_TIMEOUT=60s \
    -e RETRY_COUNT=5 \
    -e RETRY_INTERVAL=2000
```

Or with environment variables (overrides CSV):

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=tt02 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e partyUuid=*** \
    -e REQUEST_TIMEOUT=60s \
    -e RETRY_COUNT=5 \
    -e RETRY_INTERVAL=2000
```

---

## Load tests

The tests are ready for load and performance testing! When using CSV-based test data (and no environment variables are provided), each iteration automatically selects a random row from the CSV file, ensuring diverse test data across all virtual users and iterations.

> **Note:** For load testing, CSV-based approach is strongly recommended as it provides diverse test data across iterations. If environment variables are provided, they will take priority and the same test data will be used for all iterations.

Run load tests with additional parameters like `--vus` (virtual users) and `--duration` or `--iterations`:

### Example: Load test with 10 virtual users for 5 minutes (CSV-based)

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=yt01 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    --vus=10 \
    --duration=5m
```

### Example: Load test with 50 iterations (CSV-based)

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e altinn_env=yt01 \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    --vus=5 \
    --iterations=50
```

**Note:** Make sure your CSV file contains enough rows to support the number of iterations you plan to run. Each iteration randomly selects a row, so duplicates may occur, but this provides realistic load testing scenarios.

---

## Troubleshooting

Common issues and solutions:

1. **Authentication Failures**
   - Verify that tokenGeneratorUserName and tokenGeneratorUserPwd are correct
   - Check that the token generator service is accessible
   - Verify that the user has necessary permissions in the target environment

2. **Request Timeouts**
   - Increase REQUEST_TIMEOUT value
   - Check network connectivity to the target environment
   - Verify that the target environment is operational

3. **Rate Limiting**
   - Reduce the number of virtual users
   - Increase the RETRY_INTERVAL
   - Check if you're hitting API rate limits in the target environment

---

## Load test results

Test results from GitHub Actions load test runs can be found in:

- GitHub Action run logs
- Grafana dashboards (if configured)

For detailed metrics and analysis, configure the k6 output to send data to a metrics platform like Grafana.
