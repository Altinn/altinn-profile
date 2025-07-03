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

Run the test suite by specifying the filename.

For example:

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

### Command Breakdown

1. **`podman compose run`**: Runs the test in a Docker container.
2. **`k6 run {path to test file}`**: Points to the test file you want to run, e.g., `/src/tests/orders-email.js`.
3. **Script parameters**: Provided as environment variables for the container:
   ```bash
   -e tokenGeneratorUserName=***
   -e tokenGeneratorUserPwd=***
   -e env=***
   -e userID=***
   -e partyId=***
   -e pid=***
   -e partyUuid=***
   ```

---


## Load tests

The same tests can be used to run load and performance tests. These can be executed as described above, but with additional parameters like `--vus` (virtual users) and `--duration` or `--iterations`. 

You can also disable the `runFullTestSet` parameter (or set it to `false`).

For example:

Run a test with 10 virtual users (VUs) for 5 minutes:

```bash
$> k6 run /src/tests/favorites.js \
    -e tokenGeneratorUserName=*** \
    -e tokenGeneratorUserPwd=*** \
    -e env=*** \
    --vus=10 \
    --duration=5m
```

---


## Load test results

Test results from GitHub Actions load test runs can be found in:

- GitHub Action run logs
- Grafana dashboards (if configured)
