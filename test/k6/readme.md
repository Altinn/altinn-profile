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
| `env` | Environment to run tests against (prod/tt02/yt01/at22/at23/at24) | Yes | - |
| `tokenGeneratorUserName` | Username for token generator | Yes | - |
| `tokenGeneratorUserPwd` | Password for token generator | Yes | - |
| `userID` | User ID for testing | Yes | - |
| `partyId` | Party ID for testing | Yes | - |
| `pid` | Personal ID for testing | Yes | - |
| `partyUuid` | Party UUID for testing | Yes | - |
| `orgNo` | Organization number for testing | Yes | - |

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

Run a single test:

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e env=*** \
    -e userID=*** \
    -e pid=*** \
    -e partyId=*** \
    -e pid=*** \
    -e partyUuid=***
```

### Advanced Configuration

Run with custom timeout and retry settings:

```bash
$> podman compose run k6 run /src/tests/favorites.js \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e env=tt02 \
    -e REQUEST_TIMEOUT=60s \
    -e RETRY_COUNT=5 \
    -e RETRY_INTERVAL=2000
```

---

## Load tests
> [!WARNING]  
> Load testing is not supported yet.

The same tests can be used to run load and performance tests. These can be executed as described above, but with additional parameters like `--vus` (virtual users) and `--duration` or `--iterations`. 

For example:

Run a test with 10 virtual users (VUs) for 5 minutes:

```bash
$> k6 run /src/tests/favorites.js \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e env=tt02 \
    --vus=10 \
    --duration=5m
```

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
