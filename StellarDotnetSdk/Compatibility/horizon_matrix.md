# Horizon API vs Stellar .NET SDK Compatibility Matrix

**Horizon Version:** v25.0.0 (released 2025-12-11)  
**Horizon Source:** [v25.0.0](https://github.com/stellar/stellar-horizon/releases/tag/v25.0.0)  
**SDK:** `StellarDotnetSdk`  
**SDK Version:** 14.0.0  
**Updated:** 2026-03-27

**Public API Endpoints (in matrix):** 50

> **Note:** 2 endpoints are intentionally excluded from the matrix:
> - `GET /paths` - Deprecated in Horizon. This SDK has `PathsRequestBuilder`, but it is **not exposed** via `Server` (see `StellarDotnetSdk/Requests/PathsRequestBuilder.cs` and `StellarDotnetSdk/Server.cs`).
> - `POST /friendbot` - Not implemented in this SDK (SDK supports `GET /friendbot` only).

## Overall Coverage

**Coverage:** 100.0% (50/50 fully supported)

- ✅ **Fully Supported:** 50/50
- ⚠️ **Partially Supported:** 0/50
- ❌ **Not Supported:** 0/50
- 🔄 **Deprecated:** 0/50 (deprecated endpoints are excluded; see note above)

## Coverage by Category

| Category | Coverage | Fully Supported | Partially Supported | Not Supported | Total |
|----------|----------|-----------------|---------------------|---------------|-------|
| root | 100.0% | 1 | 0 | 0 | 1 |
| accounts | 100.0% | 9 | 0 | 0 | 9 |
| assets | 100.0% | 1 | 0 | 0 | 1 |
| claimable_balances | 100.0% | 4 | 0 | 0 | 4 |
| effects | 100.0% | 1 | 0 | 0 | 1 |
| fee_stats | 100.0% | 1 | 0 | 0 | 1 |
| friendbot | 100.0% | 1 | 0 | 0 | 1 |
| health | 100.0% | 1 | 0 | 0 | 1 |
| ledgers | 100.0% | 6 | 0 | 0 | 6 |
| liquidity_pools | 100.0% | 6 | 0 | 0 | 6 |
| offers | 100.0% | 3 | 0 | 0 | 3 |
| operations | 100.0% | 3 | 0 | 0 | 3 |
| order_book | 100.0% | 1 | 0 | 0 | 1 |
| paths | 100.0% | 2 | 0 | 0 | 2 |
| payments | 100.0% | 1 | 0 | 0 | 1 |
| trade_aggregations | 100.0% | 1 | 0 | 0 | 1 |
| trades | 100.0% | 1 | 0 | 0 | 1 |
| transactions | 100.0% | 6 | 0 | 0 | 6 |
| transactions_async | 100.0% | 1 | 0 | 0 | 1 |

## Streaming Support (SDK Surface)

**Coverage:** 44.0% (22/50 endpoints expose an SDK streaming API)

> Streaming in this matrix means: the endpoint is reachable via a request builder that inherits `RequestBuilderStreamable<,>` and therefore exposes `.Stream(...)` (see `StellarDotnetSdk/Requests/RequestBuilderStreamable.cs`).

- Streaming endpoints (in matrix): 22
- Supported by SDK: 22

## Detailed Endpoint Comparison

### Root

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/` | GET | ✅ | `Server.RootAsync()` / `Server.Root()` |  | Implemented on `Server` via `HttpClient.GetAsync(_serverUri)` |

### Accounts

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/accounts` | GET | ✅ | `Server.Accounts.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/accounts/{account_id}` | GET | ✅ | `Server.Accounts.Account(accountId)` |  | Direct single-resource fetch |
| `/accounts/{account_id}/data/{key}` | GET | ✅ | `Server.Accounts.AccountData(accountId, key)` |  | Direct single-resource fetch |
| `/accounts/{account_id}/effects` | GET | ✅ | `Server.Effects.ForAccount(accountId).Execute()` | ✓ | Streamable via `EffectsRequestBuilder : RequestBuilderStreamable<,>` |
| `/accounts/{account_id}/offers` | GET | ✅ | `Server.Offers.ForAccount(accountId).Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/accounts/{account_id}/operations` | GET | ✅ | `Server.Operations.ForAccount(accountId).Execute()` | ✓ | Streamable via `OperationsRequestBuilder : RequestBuilderStreamable<,>` |
| `/accounts/{account_id}/payments` | GET | ✅ | `Server.Payments.ForAccount(accountId).Execute()` | ✓ | Streamable via `PaymentsRequestBuilder : RequestBuilderStreamable<,>` |
| `/accounts/{account_id}/trades` | GET | ✅ | `Server.Trades.ForAccount(accountId).Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/accounts/{account_id}/transactions` | GET | ✅ | `Server.Transactions.ForAccount(accountId).Execute()` | ✓ | Streamable via `TransactionsRequestBuilder : RequestBuilderStreamable<,>` |

### Assets

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/assets` | GET | ✅ | `Server.Assets.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |

### Claimable_Balances

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/claimable_balances` | GET | ✅ | `Server.ClaimableBalances.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/claimable_balances/{claimable_balance_id}` | GET | ✅ | `Server.ClaimableBalances.ClaimableBalance(claimableBalanceId)` |  | Direct single-resource fetch |
| `/claimable_balances/{claimable_balance_id}/operations` | GET | ✅ | `Server.Operations.ForClaimableBalance(claimableBalanceId).Execute()` | ✓ | Streamable via `OperationsRequestBuilder` |
| `/claimable_balances/{claimable_balance_id}/transactions` | GET | ✅ | `Server.Transactions.ForClaimableBalance(claimableBalanceId).Execute()` | ✓ | Streamable via `TransactionsRequestBuilder` |

### Effects

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/effects` | GET | ✅ | `Server.Effects.Execute()` | ✓ | Streamable via `EffectsRequestBuilder` |

### Fee_Stats

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/fee_stats` | GET | ✅ | `Server.FeeStats.Execute()` |  | Direct execute |

### Friendbot

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/friendbot` | GET | ✅ | `Server.TestNetFriendBot.FundAccount(accountId).Execute()` |  | Throws if `Network.Current` is null or public network; testnet/futurenet only |

### Health

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/health` | GET | ✅ | `Server.Health.Execute()` |  | Direct execute via `HealthRequestBuilder` |

### Ledgers

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/ledgers` | GET | ✅ | `Server.Ledgers.Execute()` | ✓ | Streamable via `LedgersRequestBuilder : RequestBuilderStreamable<,>` |
| `/ledgers/{ledger_id}` | GET | ✅ | `Server.Ledgers.Ledger(ledgerSeq)` |  | Direct single-resource fetch |
| `/ledgers/{ledger_id}/effects` | GET | ✅ | `Server.Effects.ForLedger(ledgerSeq).Execute()` | ✓ | Streamable via `EffectsRequestBuilder` |
| `/ledgers/{ledger_id}/operations` | GET | ✅ | `Server.Operations.ForLedger(ledgerSeq).Execute()` | ✓ | Streamable via `OperationsRequestBuilder` |
| `/ledgers/{ledger_id}/payments` | GET | ✅ | `Server.Payments.ForLedger(ledgerSeq).Execute()` | ✓ | Streamable via `PaymentsRequestBuilder` |
| `/ledgers/{ledger_id}/transactions` | GET | ✅ | `Server.Transactions.ForLedger(ledgerSeq).Execute()` | ✓ | Streamable via `TransactionsRequestBuilder` |

### Liquidity_Pools

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/liquidity_pools` | GET | ✅ | `Server.LiquidityPools.Execute()` | ✓ | Streamable via `LiquidityPoolsRequestBuilder : RequestBuilderStreamable<,>` |
| `/liquidity_pools/{liquidity_pool_id}` | GET | ✅ | `Server.LiquidityPools.LiquidityPool(liquidityPoolId)` |  | Direct single-resource fetch |
| `/liquidity_pools/{liquidity_pool_id}/effects` | GET | ✅ | `Server.Effects.ForLiquidityPool(liquidityPoolId).Execute()` | ✓ | Streamable via `EffectsRequestBuilder` |
| `/liquidity_pools/{liquidity_pool_id}/operations` | GET | ✅ | `Server.Operations.ForLiquidityPool(liquidityPoolId).Execute()` | ✓ | Streamable via `OperationsRequestBuilder` |
| `/liquidity_pools/{liquidity_pool_id}/trades` | GET | ✅ | `Server.Trades.ForLiquidityPool(liquidityPoolId).Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/liquidity_pools/{liquidity_pool_id}/transactions` | GET | ✅ | `Server.Transactions.ForLiquidityPool(liquidityPoolId).Execute()` | ✓ | Streamable via `TransactionsRequestBuilder` |

### Offers

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/offers` | GET | ✅ | `Server.Offers.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/offers/{offer_id}` | GET | ✅ | `Server.Offers.Offer(offerId)` |  | Direct single-resource fetch |
| `/offers/{offer_id}/trades` | GET | ✅ | `Server.Trades.ForOffer(offerId).Execute()` |  | Direct sub-resource via `TradesRequestBuilder.ForOffer()` |

### Operations

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/operations` | GET | ✅ | `Server.Operations.Execute()` | ✓ | Streamable via `OperationsRequestBuilder` |
| `/operations/{operation_id}` | GET | ✅ | `OperationsRequestBuilder.Operation(long)` + `OperationsRequestBuilder.Operation(Uri)` |  | Requires building the URI then calling `Operation(uri)` |
| `/operations/{operation_id}/effects` | GET | ✅ | `Server.Effects.ForOperation(operationId).Execute()` | ✓ | Streamable via `EffectsRequestBuilder` |

### Order_Book

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/order_book` | GET | ✅ | `Server.OrderBook.BuyingAsset(...).SellingAsset(...).Execute()` |  | Not streamable (does not inherit `RequestBuilderStreamable`) |

### Paths

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/paths/strict-receive` | GET | ✅ | `Server.PathStrictReceive.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |
| `/paths/strict-send` | GET | ✅ | `Server.PathStrictSend.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |

### Payments

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/payments` | GET | ✅ | `Server.Payments.Execute()` | ✓ | Streamable via `PaymentsRequestBuilder` |

### Trade_Aggregations

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/trade_aggregations` | GET | ✅ | `Server.TradeAggregations.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |

### Trades

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/trades` | GET | ✅ | `Server.Trades.Execute()` |  | Pageable via `RequestBuilderExecutePageable` |

### Transactions

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/transactions` | GET | ✅ | `Server.Transactions.Execute()` | ✓ | Streamable via `TransactionsRequestBuilder` |
| `/transactions` | POST | ✅ | `Server.SubmitTransaction(...)` |  | Implemented on `Server` (POST `tx=` form field to `/transactions`) |
| `/transactions/{transaction_id}` | GET | ✅ | `Server.Transactions.Transaction(transactionId)` |  | Direct single-resource fetch |
| `/transactions/{transaction_id}/effects` | GET | ✅ | `Server.Effects.ForTransaction(transactionId).Execute()` | ✓ | Streamable via `EffectsRequestBuilder` |
| `/transactions/{transaction_id}/operations` | GET | ✅ | `Server.Operations.ForTransaction(transactionId).Execute()` | ✓ | Streamable via `OperationsRequestBuilder` |
| `/transactions/{transaction_id}/payments` | GET | ✅ | `Server.Payments.ForTransaction(transactionId).Execute()` | ✓ | Streamable via `PaymentsRequestBuilder` |

### Transactions_Async

| Endpoint | Method | Status | SDK Method | Streaming | Notes |
|----------|--------|--------|------------|-----------|-------|
| `/transactions_async` | POST | ✅ | `Server.SubmitTransactionAsync(...)` |  | Implemented on `Server` (POST `tx=` form field to `/transactions_async`) |

## Legend

- ✅ **Fully Supported**: Complete implementation with all features
- ⚠️ **Partially Supported**: Basic functionality with some limitations
- ❌ **Not Supported**: Endpoint not implemented
- 🔄 **Deprecated**: Deprecated endpoint with alternative available