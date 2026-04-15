# SEP-0024 (Hosted Deposit and Withdrawal) Compatibility Matrix

**Updated:** 2026-04-15  
**SDK:** StellarDotnetSdk  
**SDK Version:** 12.0.0  
**SEP Version:** 3.8.0  
**SEP Status:** Active  
**SEP URL:** https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0024.md

## SEP Summary

This SEP defines the standard way for anchors and wallets to interact on behalf
of users. This improves user experience by allowing wallets and other clients
to interact with anchors directly without the user needing to leave the wallet
to go to the anchor's site. It is based on [SEP-0006](sep-0006.md), but only
supports the interactive flow, and cleans up or removes confusing artifacts. If
you are updating from SEP-0006 see the
[changes from SEP-6](#changes-from-SEP-6) at the bottom of this document.

## Overall Coverage

**Total Coverage:** 100.0% (94/94 fields)

- ✅ **Implemented:** 94/94
- ❌ **Not Implemented:** 0/94

**Required Fields:** 100.0% (24/24)

**Optional Fields:** 100.0% (70/70)

## Implementation Status

✅ **Implemented**

### Implementation Files

- `StellarDotnetSdk/Sep/Sep0024/InteractiveService.cs`
- `StellarDotnetSdk/Sep/Sep0024/Requests/DepositRequest.cs`
- `StellarDotnetSdk/Sep/Sep0024/Requests/WithdrawRequest.cs`
- `StellarDotnetSdk/Sep/Sep0024/Requests/FeeRequest.cs`
- `StellarDotnetSdk/Sep/Sep0024/Requests/TransactionRequest.cs`
- `StellarDotnetSdk/Sep/Sep0024/Requests/TransactionsRequest.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/InfoResponse.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/DepositAsset.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/WithdrawAsset.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/FeeEndpointInfo.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/FeatureFlags.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/InteractiveResponse.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/Transaction.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/TransactionResponse.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/TransactionsResponse.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/FeeResponse.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/FeeBreakdown.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/FeeDetails.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/Refund.cs`
- `StellarDotnetSdk/Sep/Sep0024/Responses/RefundPayment.cs`
- `StellarDotnetSdk/Sep/Sep0024/Exceptions/AuthenticationRequiredException.cs`
- `StellarDotnetSdk/Sep/Sep0024/Exceptions/RequestException.cs`
- `StellarDotnetSdk/Sep/Sep0024/Exceptions/TransactionNotFoundException.cs`

### Key Classes

- **`InteractiveService`**: Main service for SEP-24 hosted deposit and withdrawal
- **`DepositAsset`**: Asset configuration for interactive deposits
- **`WithdrawAsset`**: Asset configuration for interactive withdrawals
- **`FeeEndpointInfo`**: Fee endpoint configuration from /info response
- **`FeatureFlags`**: Feature flags indicating anchor capabilities
- **`InfoResponse`**: Response from /info endpoint with supported assets and features
- **`FeeRequest`**: Request parameters for fee calculation
- **`FeeResponse`**: Response containing calculated fee
- **`DepositRequest`**: Request for initiating interactive deposit
- **`InteractiveResponse`**: Response with interactive URL for deposit/withdrawal
- **`WithdrawRequest`**: Request for initiating interactive withdrawal
- **`TransactionsRequest`**: Request for transaction history
- **`Transaction`**: Represents a single SEP-24 transaction with full details
- **`TransactionsResponse`**: Response containing transaction list
- **`Refund`**: Refund information for a transaction
- **`RefundPayment`**: Individual refund payment details
- **`TransactionRequest`**: Request for single transaction status
- **`TransactionResponse`**: Response containing single transaction details
- **`RequestException`**: Exception for general request errors
- **`AuthenticationRequiredException`**: Exception when SEP-10 authentication is required
- **`TransactionNotFoundException`**: Exception when requested transaction is not found

## Coverage by Section

| Section | Coverage | Required Coverage | Implemented | Not Implemented | Total |
|---------|----------|-------------------|-------------|-----------------|-------|
| Deposit Asset Fields | 100.0% | 100.0% | 6 | 0 | 6 |
| Deposit Request Parameters | 100.0% | 100.0% | 12 | 0 | 12 |
| Feature Flags Fields | 100.0% | 100% | 2 | 0 | 2 |
| Fee Endpoint | 100.0% | 100% | 1 | 0 | 1 |
| Fee Endpoint Info Fields | 100.0% | 100.0% | 2 | 0 | 2 |
| Info Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Info Response Fields | 100.0% | 100.0% | 4 | 0 | 4 |
| Interactive Deposit Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Interactive Response Fields | 100.0% | 100.0% | 3 | 0 | 3 |
| Interactive Withdraw Endpoint | 100.0% | 100.0% | 1 | 0 | 1 |
| Transaction Endpoints | 100.0% | 100.0% | 2 | 0 | 2 |
| Transaction Fields | 100.0% | 100.0% | 30 | 0 | 30 |
| Transaction Status Values | 100.0% | 100.0% | 12 | 0 | 12 |
| Withdraw Asset Fields | 100.0% | 100.0% | 6 | 0 | 6 |
| Withdraw Request Parameters | 100.0% | 100.0% | 11 | 0 | 11 |

## Detailed Field Comparison

### Deposit Asset Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `enabled` | ✓ | ✅ | `Enabled` | Whether deposits are enabled for this asset |
| `fee_fixed` |  | ✅ | `FeeFixed` | Fixed deposit fee |
| `fee_minimum` |  | ✅ | `FeeMinimum` | Minimum deposit fee |
| `fee_percent` |  | ✅ | `FeePercent` | Percentage deposit fee |
| `max_amount` |  | ✅ | `MaxAmount` | Maximum deposit amount |
| `min_amount` |  | ✅ | `MinAmount` | Minimum deposit amount |

### Deposit Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `Account` | Stellar or muxed account for receiving deposit |
| `amount` |  | ✅ | `Amount` | Amount of asset to deposit |
| `asset_code` | ✓ | ✅ | `AssetCode` | Code of the Stellar asset the user wants to receive |
| `asset_issuer` |  | ✅ | `AssetIssuer` | Issuer of the Stellar asset (optional if anchor is issuer) |
| `claimable_balance_supported` |  | ✅ | `ClaimableBalanceSupported` | Whether client supports claimable balances |
| `lang` |  | ✅ | `Lang` | Language code for UI and messages (RFC 4646) |
| `memo` |  | ✅ | `Memo` | Memo value for transaction identification |
| `memo_type` |  | ✅ | `MemoType` | Type of memo (text, id, or hash) |
| `quote_id` |  | ✅ | `QuoteId` | ID from SEP-38 quote (for asset exchange) |
| `source_asset` |  | ✅ | `SourceAsset` | Off-chain asset user wants to deposit (in SEP-38 format) |
| `wallet_name` |  | ✅ | `WalletName` | Name of wallet for user communication |
| `wallet_url` |  | ✅ | `WalletUrl` | URL to link in transaction notifications |

### Feature Flags Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account_creation` |  | ✅ | `AccountCreation` | Whether anchor supports creating accounts |
| `claimable_balances` |  | ✅ | `ClaimableBalances` | Whether anchor supports claimable balances |

### Fee Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `fee_endpoint` |  | ✅ | `FeeAsync` | GET /fee - Calculates fees for a deposit or withdrawal operation (optional) |

### Fee Endpoint Info Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `authentication_required` |  | ✅ | `AuthenticationRequired` | Whether authentication is required for fee endpoint |
| `enabled` | ✓ | ✅ | `Enabled` | Whether fee endpoint is available |

### Info Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `info_endpoint` | ✓ | ✅ | `InfoAsync` | GET /info - Provides anchor capabilities and supported assets for interactive deposits/withdrawals |

### Info Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `deposit` | ✓ | ✅ | `DepositAssets` | Map of asset codes to deposit asset information |
| `features` |  | ✅ | `FeatureFlags` | Feature flags object |
| `fee` |  | ✅ | `FeeEndpointInfo` | Fee endpoint information object |
| `withdraw` | ✓ | ✅ | `WithdrawAssets` | Map of asset codes to withdraw asset information |

### Interactive Deposit Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `interactive_deposit` | ✓ | ✅ | `DepositAsync` | POST /transactions/deposit/interactive - Initiates an interactive deposit transaction |

### Interactive Response Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `id` | ✓ | ✅ | `Id` | Unique transaction identifier |
| `type` | ✓ | ✅ | `Type` | Always "interactive_customer_info_needed" for SEP-24 |
| `url` | ✓ | ✅ | `Url` | URL for interactive flow popup/iframe |

### Interactive Withdraw Endpoint

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `interactive_withdraw` | ✓ | ✅ | `WithdrawAsync` | POST /transactions/withdraw/interactive - Initiates an interactive withdrawal transaction |

### Transaction Endpoints

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `transaction` | ✓ | ✅ | `TransactionAsync` | GET /transaction - Retrieves details for a single transaction |
| `transactions` | ✓ | ✅ | `TransactionsAsync` | GET /transactions - Retrieves transaction history for authenticated account |

### Transaction Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `amount_fee` |  | ✅ | `AmountFee` | Total fee charged for transaction |
| `amount_fee_asset` |  | ✅ | `AmountFeeAsset` | Asset in which fees are calculated (SEP-38 format) |
| `amount_in` |  | ✅ | `AmountIn` | Amount received by anchor |
| `amount_in_asset` |  | ✅ | `AmountInAsset` | Asset received by anchor (SEP-38 format) |
| `amount_out` |  | ✅ | `AmountOut` | Amount sent by anchor to user |
| `amount_out_asset` |  | ✅ | `AmountOutAsset` | Asset delivered to user (SEP-38 format) |
| `claimable_balance_id` |  | ✅ | `ClaimableBalanceId` | ID of claimable balance for deposit |
| `completed_at` |  | ✅ | `CompletedAt` | When transaction completed (ISO 8601) |
| `deposit_memo` |  | ✅ | `DepositMemo` | Memo for deposit to Stellar address |
| `deposit_memo_type` |  | ✅ | `DepositMemoType` | Type of deposit memo |
| `external_transaction_id` |  | ✅ | `ExternalTransactionId` | Identifier from external system |
| `from` |  | ✅ | `From` | Source address (Stellar for withdrawals, external for deposits) |
| `id` | ✓ | ✅ | `Id` | Unique transaction identifier |
| `kind` | ✓ | ✅ | `Kind` | Kind of transaction (deposit or withdrawal) |
| `kyc_verified` |  | ✅ | `KycVerified` | Whether KYC has been verified for this transaction |
| `message` |  | ✅ | `Message` | Human-readable message about transaction |
| `more_info_url` | ✓ | ✅ | `MoreInfoUrl` | URL with additional transaction information |
| `quote_id` |  | ✅ | `QuoteId` | ID of SEP-38 quote used for this transaction |
| `refunded` |  | ✅ | `Refunded` | Whether transaction was refunded (deprecated) |
| `refunds` |  | ✅ | `Refunds` | Refund information object |
| `started_at` | ✓ | ✅ | `StartedAt` | When transaction was created (ISO 8601) |
| `status` | ✓ | ✅ | `Status` | Current status of the transaction |
| `status_eta` |  | ✅ | `StatusEta` | Estimated seconds until status changes |
| `stellar_transaction_id` |  | ✅ | `StellarTransactionId` | Hash of the Stellar transaction |
| `to` |  | ✅ | `To` | Destination address (Stellar for deposits, external for withdrawals) |
| `updated_at` |  | ✅ | `UpdatedAt` | When transaction status last changed (ISO 8601) |
| `user_action_required_by` |  | ✅ | `UserActionRequiredBy` | Deadline for user action (ISO 8601) |
| `withdraw_anchor_account` |  | ✅ | `WithdrawAnchorAccount` | Anchor's Stellar account for withdrawal payment |
| `withdraw_memo` |  | ✅ | `WithdrawMemo` | Memo for withdrawal to anchor account |
| `withdraw_memo_type` |  | ✅ | `WithdrawMemoType` | Type of withdraw memo |

### Transaction Status Values

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `completed` | ✓ | ✅ | `Status` | Transaction completed successfully |
| `error` |  | ✅ | `Status` | Transaction encountered an error |
| `expired` |  | ✅ | `Status` | Transaction expired before completion |
| `incomplete` | ✓ | ✅ | `Status` | Customer information still being collected via interactive flow |
| `pending_anchor` | ✓ | ✅ | `Status` | Anchor processing the transaction |
| `pending_external` |  | ✅ | `Status` | Transaction being processed by external system |
| `pending_stellar` |  | ✅ | `Status` | Transaction submitted to Stellar network |
| `pending_trust` |  | ✅ | `Status` | User needs to establish trustline |
| `pending_user` |  | ✅ | `Status` | Waiting for user action (e.g., accepting claimable balance) |
| `pending_user_transfer_complete` |  | ✅ | `Status` | User transfer detected, awaiting confirmations |
| `pending_user_transfer_start` | ✓ | ✅ | `Status` | Waiting for user to send funds (deposits) |
| `refunded` |  | ✅ | `Status` | Transaction refunded |

### Withdraw Asset Fields

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `enabled` | ✓ | ✅ | `Enabled` | Whether withdrawals are enabled for this asset |
| `fee_fixed` |  | ✅ | `FeeFixed` | Fixed withdrawal fee |
| `fee_minimum` |  | ✅ | `FeeMinimum` | Minimum withdrawal fee |
| `fee_percent` |  | ✅ | `FeePercent` | Percentage withdrawal fee |
| `max_amount` |  | ✅ | `MaxAmount` | Maximum withdrawal amount |
| `min_amount` |  | ✅ | `MinAmount` | Minimum withdrawal amount |

### Withdraw Request Parameters

| Field | Required | Status | SDK Property | Description |
|-------|----------|--------|--------------|-------------|
| `account` |  | ✅ | `Account` | Stellar or muxed account that will send the withdrawal |
| `amount` |  | ✅ | `Amount` | Amount of asset to withdraw |
| `asset_code` | ✓ | ✅ | `AssetCode` | Code of the Stellar asset user wants to send |
| `asset_issuer` |  | ✅ | `AssetIssuer` | Issuer of the Stellar asset (optional if anchor is issuer) |
| `destination_asset` |  | ✅ | `DestinationAsset` | Off-chain asset user wants to receive (in SEP-38 format) |
| `lang` |  | ✅ | `Lang` | Language code for UI and messages (RFC 4646) |
| `memo` |  | ✅ | `Memo` | Memo for identifying the withdrawal transaction |
| `memo_type` |  | ✅ | `MemoType` | Type of memo (text, id, or hash) |
| `quote_id` |  | ✅ | `QuoteId` | ID from SEP-38 quote (for asset exchange) |
| `wallet_name` |  | ✅ | `WalletName` | Name of wallet for user communication |
| `wallet_url` |  | ✅ | `WalletUrl` | URL to link in transaction notifications |

## Implementation Gaps

🎉 **No gaps found!** All fields are implemented.

## Recommendations

✅ The SDK has full compatibility with SEP-0024!

## Legend

- ✅ **Implemented**: Field is implemented in SDK
- ❌ **Not Implemented**: Field is missing from SDK
- ⚙️ **Server**: Server-side only feature (not applicable to client SDKs)
- ✓ **Required**: Field is required by SEP specification
- (blank) **Optional**: Field is optional
