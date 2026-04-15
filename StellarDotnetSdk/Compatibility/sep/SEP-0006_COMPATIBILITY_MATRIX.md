# SEP-0006 (Deposit and Withdrawal API) Compatibility Matrix

**Updated:** 2026-04-15  
**SDK:** StellarDotnetSdk  
**SDK Version:** 12.0.0  
**SEP Version:** 4.3.0  
**SEP Status:** Active (Interactive components are deprecated in favor of SEP-24)  
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0006.md

## SEP Summary

This SEP defines the standard way for anchors and wallets to interact on behalf
of users. This improves user experience by allowing wallets and other clients
to interact with anchors directly without the user needing to leave the wallet
to go to the anchor's site.

Please note that this SEP provides a normalized interface specification that
allows wallets and other services to interact with anchors _programmatically_.
[SEP-24](sep-0024.md) was created to support use cases where the anchor may
want to interact with users _interactively_ using a popup opened within the
wallet application.

## Overall Coverage

**Total Coverage:** 100.0% (95/95 fields)

- ✅ **Implemented:** 95/95
- ❌ **Not Implemented:** 0/95

**Required Fields:** 100.0% (22/22)

**Optional Fields:** 100.0% (73/73)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0006/TransferServerService.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/DepositRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/DepositExchangeRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/WithdrawRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/WithdrawExchangeRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/AnchorTransactionRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/AnchorTransactionsRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/FeeRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Requests/PatchTransactionRequest.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/InfoResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/DepositResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/WithdrawResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/DepositInstruction.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/DepositAsset.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/DepositExchangeAsset.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/WithdrawAsset.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/WithdrawExchangeAsset.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorTransaction.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorTransactionResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorTransactionsResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorField.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorFeeInfo.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorFeatureFlags.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorTransactionInfo.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/AnchorTransactionsInfo.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/FeeResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/FeeDetails.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/FeeDetailsDetails.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/TransactionRefunds.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/TransactionRefundPayment.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/ExtraInfo.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/CustomerInformationNeededResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/CustomerInformationStatusResponse.cs`
- `StellarDotnetSdk/Sep/Sep0006/Responses/RequiredInfoUpdates.cs`
- `StellarDotnetSdk/Sep/Sep0006/Exceptions/CustomerInformationNeededException.cs`
- `StellarDotnetSdk/Sep/Sep0006/Exceptions/CustomerInformationStatusException.cs`
- `StellarDotnetSdk/Sep/Sep0006/Exceptions/AuthenticationRequiredException.cs`
- `StellarDotnetSdk/Sep/Sep0006/Exceptions/TransferServerException.cs`

### Key Classes

- **`TransferServerService`**: Main service class for SEP-6 deposit and withdrawal operations
- **`DepositRequest`**: Request parameters for initiating a deposit
- **`DepositResponse`**: Response containing deposit instructions from anchor
- **`DepositInstruction`**: Instructions for completing a deposit (account, memo, etc.)
- **`ExtraInfo`**: Additional information provided by anchor
- **`CustomerInformationNeededResponse`**: Response when additional KYC info is required
- **`CustomerInformationNeededException`**: Exception thrown when KYC info is needed
- **`CustomerInformationStatusResponse`**: Response with KYC verification status
- **`CustomerInformationStatusException`**: Exception for KYC status issues
- **`AuthenticationRequiredException`**: Exception when SEP-10 authentication is required
- **`DepositExchangeRequest`**: Request for deposit with on-chain asset exchange
- **`WithdrawRequest`**: Request parameters for initiating a withdrawal
- **`WithdrawExchangeRequest`**: Request for withdrawal with on-chain asset exchange
- **`WithdrawResponse`**: Response containing withdrawal details from anchor
- **`AnchorField`**: Custom field definition required by anchor for KYC/compliance
- **`DepositAsset`**: Asset configuration for deposits (min/max amounts, fees, etc.)
- **`DepositExchangeAsset`**: Asset configuration for deposit-exchange operations
- **`WithdrawAsset`**: Asset configuration for withdrawals (min/max amounts, fees, etc.)
- **`WithdrawExchangeAsset`**: Asset configuration for withdraw-exchange operations
- **`AnchorFeeInfo`**: Fee information from anchor /info endpoint
- **`AnchorTransactionInfo`**: Transaction information from /transaction endpoint
- **`AnchorTransactionsInfo`**: Transaction list from /transactions endpoint
- **`AnchorFeatureFlags`**: Feature flags indicating anchor capabilities
- **`InfoResponse`**: Response from /info endpoint with supported assets and features
- **`FeeRequest`**: Request parameters for fee calculation
- **`FeeResponse`**: Response containing calculated fee for operation
- **`AnchorTransactionsRequest`**: Request for transaction history
- **`FeeDetails`**: Detailed fee breakdown information
- **`FeeDetailsDetails`**: Individual fee component details
- **`TransactionRefunds`**: Refund information for a transaction
- **`TransactionRefundPayment`**: Individual refund payment details
- **`AnchorTransaction`**: Represents a single anchor transaction with full details
- **`AnchorTransactionsResponse`**: Response containing transaction list
- **`AnchorTransactionRequest`**: Request for single transaction status
- **`AnchorTransactionResponse`**: Response containing single transaction details
- **`PatchTransactionRequest`**: Request to update transaction with additional info

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Deposit Endpoints | 100.0% | 100.0% | 2 | 0 | 2 |
| Deposit Request Parameters | 100.0% | 100.0% | 15 | 0 | 15 |
| Deposit Response Fields | 100.0% | 100.0% | 8 | 0 | 8 |
| Fee Endpoint | 100.0% | 100% | 1 | 0 | 1 |
| Info Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Info Response Fields | 100.0% | 100.0% | 8 | 0 | 8 |
| Transaction Endpoints | 100.0% | 100.0% | 3 | 0 | 3 |
| Transaction Fields | 100.0% | 100.0% | 16 | 0 | 16 |
| Transaction Status Values | 100.0% | 100.0% | 12 | 0 | 12 |
| Withdraw Endpoints | 100.0% | 100.0% | 2 | 0 | 2 |
| Withdraw Request Parameters | 100.0% | 100.0% | 17 | 0 | 17 |
| Withdraw Response Fields | 100.0% | 100.0% | 10 | 0 | 10 |

## Detailed Field Comparison

### Deposit Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `deposit` | ✓ | ✅ | `DepositAsync` | GET /deposit - Initiates a deposit transaction for on-chain assets |
| `deposit_exchange` |  | ✅ | `DepositExchangeAsync` | GET /deposit-exchange - Initiates a deposit with asset exchange (SEP-38 integration) |

### Deposit Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` | ✓ | ✅ | `DepositRequest.Account` | Stellar account ID of the user |
| `amount` |  | ✅ | `DepositRequest.Amount` | Amount of on-chain asset the user wants to receive |
| `asset_code` | ✓ | ✅ | `DepositRequest.AssetCode` | Code of the on-chain asset the user wants to receive |
| `claimable_balance_supported` |  | ✅ | `DepositRequest.ClaimableBalanceSupported` | Whether the client supports receiving claimable balances |
| `country_code` |  | ✅ | `DepositRequest.CountryCode` | Country code of the user (ISO 3166-1 alpha-3) |
| `customer_id` |  | ✅ | `DepositRequest.CustomerId` | ID of the customer from SEP-12 KYC process |
| `email_address` |  | ✅ | `DepositRequest.EmailAddress` | Email address of the user (for notifications) |
| `lang` |  | ✅ | `DepositRequest.Lang` | Language code for response messages (ISO 639-1) |
| `location_id` |  | ✅ | `DepositRequest.LocationId` | ID of the physical location for cash pickup |
| `memo` |  | ✅ | `DepositRequest.Memo` | Value of memo to attach to transaction |
| `memo_type` |  | ✅ | `DepositRequest.MemoType` | Type of memo to attach to transaction (text, id, or hash) |
| `on_change_callback` |  | ✅ | `DepositRequest.OnChangeCallback` | URL for anchor to send callback when transaction status changes |
| `type` |  | ✅ | `DepositRequest.Type` | Type of deposit method (e.g., bank_account, cash, mobile_money) |
| `wallet_name` |  | ✅ | `DepositRequest.WalletName` | Name of the wallet the user is using |
| `wallet_url` |  | ✅ | `DepositRequest.WalletUrl` | URL of the wallet the user is using |

### Deposit Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `eta` |  | ✅ | `DepositResponse.Eta` | Estimated seconds until deposit completes |
| `extra_info` |  | ✅ | `DepositResponse.ExtraInfo` | Additional information about the deposit |
| `fee_fixed` |  | ✅ | `DepositResponse.FeeFixed` | Fixed fee for deposit |
| `fee_percent` |  | ✅ | `DepositResponse.FeePercent` | Percentage fee for deposit |
| `how` | ✓ | ✅ | `DepositResponse.How` | Instructions for how to deposit the asset |
| `id` |  | ✅ | `DepositResponse.Id` | Persistent transaction identifier |
| `max_amount` |  | ✅ | `DepositResponse.MaxAmount` | Maximum deposit amount |
| `min_amount` |  | ✅ | `DepositResponse.MinAmount` | Minimum deposit amount |

### Fee Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `fee_endpoint` |  | ✅ | `FeeAsync` | GET /fee - Calculates fees for a deposit or withdrawal operation |

### Info Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `info_endpoint` | ✓ | ✅ | `InfoAsync` | GET /info - Provides anchor capabilities and asset information |

### Info Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `deposit` | ✓ | ✅ | `InfoResponse.DepositAssets` | Map of asset codes to deposit asset information |
| `deposit-exchange` |  | ✅ | `InfoResponse.DepositExchangeAssets` | Map of asset codes to deposit-exchange asset information |
| `features` |  | ✅ | `InfoResponse.FeatureFlags` | Feature flags supported by the anchor |
| `fee` |  | ✅ | `InfoResponse.FeeInfo` | Fee endpoint information |
| `transaction` |  | ✅ | `InfoResponse.TransactionInfo` | Single transaction endpoint information |
| `transactions` |  | ✅ | `InfoResponse.TransactionsInfo` | Transaction history endpoint information |
| `withdraw` | ✓ | ✅ | `InfoResponse.WithdrawAssets` | Map of asset codes to withdraw asset information |
| `withdraw-exchange` |  | ✅ | `InfoResponse.WithdrawExchangeAssets` | Map of asset codes to withdraw-exchange asset information |

### Transaction Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `patch_transaction` |  | ✅ | `PatchTransactionAsync` | PATCH /transaction - Updates transaction fields (for debugging/testing) |
| `transaction` | ✓ | ✅ | `TransactionAsync` | GET /transaction - Retrieves details for a single transaction |
| `transactions` | ✓ | ✅ | `TransactionsAsync` | GET /transactions - Retrieves transaction history for an account |

### Transaction Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `amount_fee` |  | ✅ | `AnchorTransaction.AmountFee` | Total fee charged for transaction |
| `amount_in` |  | ✅ | `AnchorTransaction.AmountIn` | Amount received by anchor |
| `amount_out` |  | ✅ | `AnchorTransaction.AmountOut` | Amount sent by anchor to user |
| `completed_at` |  | ✅ | `AnchorTransaction.CompletedAt` | When transaction completed (ISO 8601) |
| `external_transaction_id` |  | ✅ | `AnchorTransaction.ExternalTransactionId` | Identifier from external system |
| `from` |  | ✅ | `AnchorTransaction.From` | Stellar account that initiated the transaction |
| `id` | ✓ | ✅ | `AnchorTransaction.Id` | Unique transaction identifier |
| `kind` | ✓ | ✅ | `AnchorTransaction.Kind` | Kind of transaction (deposit, withdrawal, deposit-exchange, withdrawal-exchange) |
| `message` |  | ✅ | `AnchorTransaction.Message` | Human-readable message about transaction |
| `refunded` |  | ✅ | `AnchorTransaction.Refunded` | Whether transaction was refunded |
| `refunds` |  | ✅ | `AnchorTransaction.Refunds` | Refund information if applicable |
| `started_at` | ✓ | ✅ | `AnchorTransaction.StartedAt` | When transaction was created (ISO 8601) |
| `status` | ✓ | ✅ | `AnchorTransaction.Status` | Current status of the transaction |
| `status_eta` |  | ✅ | `AnchorTransaction.StatusEta` | Estimated seconds until status changes |
| `stellar_transaction_id` |  | ✅ | `AnchorTransaction.StellarTransactionId` | Hash of the Stellar transaction |
| `to` |  | ✅ | `AnchorTransaction.To` | Stellar account receiving the transaction |

### Transaction Status Values

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `completed` | ✓ | ✅ | - | Transaction completed successfully |
| `error` |  | ✅ | - | Transaction failed with error |
| `expired` |  | ✅ | - | Transaction expired without completion |
| `incomplete` | ✓ | ✅ | - | Deposit/withdrawal has not yet been submitted |
| `pending_anchor` | ✓ | ✅ | - | Anchor is processing the transaction |
| `pending_external` |  | ✅ | - | Waiting for external action (banking system, etc.) |
| `pending_stellar` |  | ✅ | - | Stellar transaction has been submitted |
| `pending_trust` |  | ✅ | - | User needs to add trustline for asset |
| `pending_user` |  | ✅ | - | Waiting for user action (accepting claimable balance) |
| `pending_user_transfer_complete` |  | ✅ | - | Off-chain transfer has been initiated |
| `pending_user_transfer_start` | ✓ | ✅ | - | Waiting for user to initiate off-chain transfer |
| `refunded` |  | ✅ | - | Transaction refunded |

### Withdraw Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `withdraw` | ✓ | ✅ | `WithdrawAsync` | GET /withdraw - Initiates a withdrawal transaction for off-chain assets |
| `withdraw_exchange` |  | ✅ | `WithdrawExchangeAsync` | GET /withdraw-exchange - Initiates a withdrawal with asset exchange (SEP-38 integration) |

### Withdraw Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `WithdrawRequest.Account` | Stellar account ID of the user |
| `amount` |  | ✅ | `WithdrawRequest.Amount` | Amount of on-chain asset the user wants to send |
| `asset_code` | ✓ | ✅ | `WithdrawRequest.AssetCode` | Code of the on-chain asset the user wants to send |
| `country_code` |  | ✅ | `WithdrawRequest.CountryCode` | Country code of the user (ISO 3166-1 alpha-3) |
| `customer_id` |  | ✅ | `WithdrawRequest.CustomerId` | ID of the customer from SEP-12 KYC process |
| `dest` |  | ✅ | `WithdrawRequest.Dest` | Destination for withdrawal (bank account number, etc.) |
| `dest_extra` |  | ✅ | `WithdrawRequest.DestExtra` | Extra information for destination (routing number, etc.) |
| `lang` |  | ✅ | `WithdrawRequest.Lang` | Language code for response messages (ISO 639-1) |
| `location_id` |  | ✅ | `WithdrawRequest.LocationId` | ID of the physical location for cash pickup |
| `memo` |  | ✅ | `WithdrawRequest.Memo` | Memo to identify the user if account is shared |
| `memo_type` |  | ✅ | `WithdrawRequest.MemoType` | Type of memo (text, id, or hash) |
| `on_change_callback` |  | ✅ | `WithdrawRequest.OnChangeCallback` | URL for anchor to send callback when transaction status changes |
| `refund_memo` |  | ✅ | `WithdrawRequest.RefundMemo` | Memo to use for refund transaction if withdrawal fails |
| `refund_memo_type` |  | ✅ | `WithdrawRequest.RefundMemoType` | Type of refund memo (text, id, or hash) |
| `type` | ✓ | ✅ | `WithdrawRequest.Type` | Type of withdrawal method (e.g., bank_account, cash, mobile_money) |
| `wallet_name` |  | ✅ | `WithdrawRequest.WalletName` | Name of the wallet the user is using |
| `wallet_url` |  | ✅ | `WithdrawRequest.WalletUrl` | URL of the wallet the user is using |

### Withdraw Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account_id` | ✓ | ✅ | `WithdrawResponse.AccountId` | Stellar account to send withdrawn assets to |
| `eta` |  | ✅ | `WithdrawResponse.Eta` | Estimated seconds until withdrawal completes |
| `extra_info` |  | ✅ | `WithdrawResponse.ExtraInfo` | Additional information about the withdrawal |
| `fee_fixed` |  | ✅ | `WithdrawResponse.FeeFixed` | Fixed fee for withdrawal |
| `fee_percent` |  | ✅ | `WithdrawResponse.FeePercent` | Percentage fee for withdrawal |
| `id` | ✓ | ✅ | `WithdrawResponse.Id` | Persistent transaction identifier |
| `max_amount` |  | ✅ | `WithdrawResponse.MaxAmount` | Maximum withdrawal amount |
| `memo` |  | ✅ | `WithdrawResponse.Memo` | Value of memo to attach to transaction |
| `memo_type` |  | ✅ | `WithdrawResponse.MemoType` | Type of memo to attach to transaction |
| `min_amount` |  | ✅ | `WithdrawResponse.MinAmount` | Minimum withdrawal amount |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0006!

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
