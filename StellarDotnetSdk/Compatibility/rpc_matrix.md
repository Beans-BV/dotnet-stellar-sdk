# Stellar RPC vs .NET SDK Compatibility Matrix

**RPC Version:** v28.0.1 (released 2026-08-27)
**RPC Source:** [https://github.com/stellar/stellar-rpc/releases/tag/v28.0.1](https://github.com/stellar/stellar-rpc/releases/tag/v28.0.1)
**SDK Version:** 12.0.0
**Updated:** 2026-08-28

> **Version history:** RPC v26.0.1 completed the `getLatestLedger` response (`closeTime`, `headerXdr`, `metadataXdr`;
> SDK support in [#198](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/198)). RPC v27.1.0 added two optional
> `getHealth` response fields (`latestLedgerCloseTime`, `oldestLedgerCloseTime`) and an optional `simulateTransaction`
> request flag (`useUpgradedAuth`, tracked in [#206](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/206)).
> RPC v27.0.0 and v28.0.0 changed no method signatures; they updated the XDR to Protocol 27 and Protocol 28
> (CAP-0083, CAP-0085). Protocol 28 XDR is not yet regenerated in the SDK
> ([#207](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/207)), so `SCV_EXECUTABLE_TAG` values and
> `CONTRACT_EXECUTABLE_EXTERNAL_REF` executables in RPC responses cannot be decoded until that lands.

## Overall Coverage

**Coverage:** 83%

- ✅ **Fully Supported:** 10/12
- ⚠️ **Partially Supported:** 2/12
- ❌ **Not Supported:** 0/12

## Method Comparison

| RPC Method | Status | .NET Method | Required Params | Response Fields | Notes |
|------------|--------|----------------|-----------------|-----------------|-------|
| `getEvents` | ✅ Fully Supported | `getEvents` | 1/1 | 6/6 | All parameters and response fields implemented |
| `getFeeStats` | ✅ Fully Supported | `getFeeStats` | N/A | 3/3 | All parameters and response fields implemented |
| `getHealth` | ⚠️ Partially Supported | `getHealth` | N/A | 4/6 | Missing `latestLedgerCloseTime` and `oldestLedgerCloseTime` (added in RPC v27.1.0) |
| `getLatestLedger` | ✅ Fully Supported | `getLatestLedger` | N/A | 6/6 | All parameters and response fields implemented |
| `getLedgerEntries` | ✅ Fully Supported | `getLedgerEntries` | 1/1 | 2/2 | All parameters and response fields implemented |
| `getLedgers` | ✅ Fully Supported | `getLedgers` | 1/1 | 6/6 | All parameters and response fields implemented |
| `getNetwork` | ✅ Fully Supported | `getNetwork` | N/A | 3/3 | All parameters and response fields implemented |
| `getTransaction` | ✅ Fully Supported | `getTransaction` | 1/1 | 5/5 | All parameters and response fields implemented |
| `getTransactions` | ✅ Fully Supported | `getTransactions` | 1/1 | 6/6 | All parameters and response fields implemented |
| `getVersionInfo` | ✅ Fully Supported | `getVersionInfo` | N/A | 5/5 | All parameters and response fields implemented |
| `sendTransaction` | ✅ Fully Supported | `sendTransaction` | 1/1 | N/A | All parameters and response fields implemented |
| `simulateTransaction` | ⚠️ Partially Supported | `simulateTransaction` | 1/1 | N/A | Optional `useUpgradedAuth` request flag (RPC v27.1.0) not exposed, see [#206](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/206) |

## Response Field Coverage

Detailed breakdown of response field support per method.

| RPC Method | RPC Fields | SDK Fields | Missing Fields |
|------------|------------|------------|----------------|
| `getEvents` | 6 | 6 | - |
| `getFeeStats` | 3 | 3 | - |
| `getHealth` | 6 | 4 | `latestLedgerCloseTime`, `oldestLedgerCloseTime` |
| `getLatestLedger` | 6 | 6 | - |
| `getLedgerEntries` | 2 | 2 | - |
| `getLedgers` | 6 | 6 | - |
| `getNetwork` | 3 | 3 | - |
| `getTransaction` | 5 | 5 | - |
| `getTransactions` | 6 | 6 | - |
| `getVersionInfo` | 5 | 5 | - |
