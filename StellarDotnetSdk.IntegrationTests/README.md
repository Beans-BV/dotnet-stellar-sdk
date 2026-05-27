# StellarDotnetSdk.IntegrationTests

End-to-end tests that exercise the SDK against the public Stellar **Testnet**.

## When this runs

- **Locally:** `dotnet test StellarDotnetSdk.IntegrationTests/StellarDotnetSdk.IntegrationTests.csproj`
- **CI:** `.github/workflows/integration_tests.yml` runs on every `v*` tag push and on `workflow_dispatch`.
  It does **not** run on regular PRs (those use `pack_and_test.yml`, which only runs the unit project).

## Environment overrides

| Variable                      | Default                               |
|-------------------------------|---------------------------------------|
| `INTEGRATION_HORIZON_URL`     | `https://horizon-testnet.stellar.org` |
| `INTEGRATION_SOROBAN_RPC_URL` | `https://soroban-testnet.stellar.org` |

CI consumes these from repo `vars.*`; locally, set them in your shell.

## Testnet resets

Stellar Testnet is reset roughly quarterly. These tests are resilient by design:
every test generates its own keypair and funds it via Friendbot, so nothing
on-chain is shared between tests or runs.

## Friendbot rate limits

Friendbot occasionally returns 429s. `IntegrationTestBase.CreateFundedAccountAsync`
retries 3 times with exponential backoff (1s, 3s, 9s) before reporting the test
as `Inconclusive` — that signals a Testnet outage, not an SDK regression.
