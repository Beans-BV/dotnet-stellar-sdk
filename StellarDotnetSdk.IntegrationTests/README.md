# StellarDotnetSdk.IntegrationTests

End-to-end tests that exercise the SDK against the Stellar **Testnet**.

## When this runs

- **Locally:** `dotnet test StellarDotnetSdk.IntegrationTests/StellarDotnetSdk.IntegrationTests.csproj`
- **CI:** `.github/workflows/integration_tests.yml` runs on pushes to `main`, on `v*` tag pushes,
  and on `workflow_dispatch`. It does **not** run on regular PRs (those use `pack_and_test.yml`,
  which only runs the unit project).

## Environment overrides

The data plane (Horizon / Stellar RPC) defaults to Stellar's public Testnet, so outside
contributors need no configuration. To route it to an authenticated provider instead, set
these variables — URLs as repo `vars.*`, tokens as `secrets.*`:

| Variable | Default | Purpose |
|---|---|---|
| `INTEGRATION_HORIZON_URL`       | `https://horizon-testnet.stellar.org` | Horizon base URL for reads/submissions |
| `INTEGRATION_HORIZON_TOKEN`     | _(none)_                              | Bearer token for `INTEGRATION_HORIZON_URL` |
| `INTEGRATION_FRIENDBOT_URL`     | `https://horizon-testnet.stellar.org` | Horizon used for Friendbot funding (see below) |
| `INTEGRATION_STELLAR_RPC_URL`   | `https://soroban-testnet.stellar.org` | Stellar RPC base URL (used from Phase 2) |
| `INTEGRATION_STELLAR_RPC_TOKEN` | _(none)_                              | Bearer token for `INTEGRATION_STELLAR_RPC_URL` |
| `INTEGRATION_SEP10_HOME_DOMAIN` | `testanchor.stellar.org`              | Home domain of the SEP-10 anchor for the real-anchor auth test |

## Friendbot funding

Funding always uses Stellar's public Testnet Friendbot, even when Horizon/RPC point at a
provider — the Friendbot faucet is SDF-only and not hosted by most providers.
`IntegrationTestBase` uses a dedicated funding client (`INTEGRATION_FRIENDBOT_URL`) for this,
separate from the main client.

Friendbot occasionally returns 429s. `IntegrationTestBase.CreateFundedAccountAsync` makes an
initial attempt plus up to 3 backoff retries (1s/3s/9s, honoring a server `Retry-After` when
present) before reporting the test as `Inconclusive` — that signals a Testnet outage or
rate-limit, not an SDK regression.

## Testnet resets

Stellar Testnet is reset roughly quarterly. These tests are resilient by design: every test
generates its own keypair and funds it via Friendbot, so nothing on-chain is shared between
tests or runs.
