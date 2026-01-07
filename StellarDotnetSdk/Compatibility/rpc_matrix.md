# Soroban RPC vs .NET SDK Compatibility Matrix

**RPC Version:** v25.0.0 (released 2025-12-12)  
**RPC Source:** [https://github.com/stellar/stellar-rpc/releases/tag/v25.0.0](https://github.com/stellar/stellar-rpc/releases/tag/v25.0.0)  
**SDK Version:** 12.0.0  
**Generated:** 2026-01-07 13:19:17

## Overall Coverage

**Coverage:** 83.3%

- ✅ **Fully Supported:** 10/12
- ⚠️ **Partially Supported:** 1/12
- ❌ **Not Supported:** 1/12

## Method Comparison

| RPC Method | Status | .NET Method | Required Params | Response Fields | Notes |
|------------|--------|----------------|-----------------|-----------------|-------|
| `getEvents` | ✅ Fully Supported | `getEvents` | 1/1 | 6/6 | All parameters and response fields implemented |
| `getFeeStats` | ✅ Fully Supported | `getFeeStats` | N/A | 3/3 | All parameters and response fields implemented |
| `getHealth` | ✅ Fully Supported | `getHealth` | N/A | 4/4 | All parameters and response fields implemented |
| `getLatestLedger` | ✅ Fully Supported | `getLatestLedger` | N/A | 3/3 | All parameters and response fields implemented |
| `getLedgerEntries` | ✅ Fully Supported | `getLedgerEntries` | 1/1 | 2/2 | All parameters and response fields implemented |
| `getLedgers` | ❌ Not Supported | `getLedgers` | 1/1 | 6/6 | No `getLedgers` implementation in .NET SDK (`SorobanServer`) |
| `getNetwork` | ✅ Fully Supported | `getNetwork` | N/A | 3/3 | All parameters and response fields implemented |
| `getTransaction` | ✅ Fully Supported | `getTransaction` | 1/1 | 5/5 | All parameters and response fields implemented |
| `getTransactions` | ⚠️ Partially Supported | `getTransactions` | 1/1 | 5/6 | Response field `cursor` is not exposed in .NET SDK response model |
| `getVersionInfo` | ✅ Fully Supported | `getVersionInfo` | N/A | 5/5 | All parameters and response fields implemented |
| `sendTransaction` | ✅ Fully Supported | `sendTransaction` | 1/1 | N/A | All parameters and response fields implemented |
| `simulateTransaction` | ✅ Fully Supported | `simulateTransaction` | 1/1 | N/A | All parameters and response fields implemented |

## Response Field Coverage

Detailed breakdown of response field support per method.

| RPC Method | RPC Fields | SDK Fields | Missing Fields |
|------------|------------|------------|----------------|
| `getEvents` | 6 | 6 | - |
| `getFeeStats` | 3 | 3 | - |
| `getHealth` | 4 | 4 | - |
| `getLatestLedger` | 3 | 3 | - |
| `getLedgerEntries` | 2 | 2 | - |
| `getLedgers` | 6 | 0 | All fields (method not implemented) |
| `getNetwork` | 3 | 3 | - |
| `getTransaction` | 5 | 5 | - |
| `getTransactions` | 6 | 5 | `cursor` |
| `getVersionInfo` | 5 | 5 | - |
