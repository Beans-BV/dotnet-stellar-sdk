using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Represents a single deposit or withdrawal transaction with an anchor.
///     Contains comprehensive information about the transaction status, amounts,
///     fees, and relevant identifiers for tracking the transaction through its lifecycle.
///     Transaction statuses:
///     - incomplete: Additional user action required via the interactive URL
///     - pending_user_transfer_start: Waiting for user to initiate transfer
///     - pending_anchor: Anchor is processing the transaction
///     - pending_stellar: Transaction submitted to Stellar network
///     - pending_external: Pending external payment system
///     - pending_trust: Waiting for user to establish trustline
///     - pending_user: Waiting for user action
///     - completed: Transaction successfully completed
///     - refunded: Transaction was refunded
///     - expired: Transaction expired before completion
///     - error: Transaction failed with an error
/// </summary>
public class Sep24Transaction : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24Transaction" /> class.
    /// </summary>
    [JsonConstructor]
    public Sep24Transaction(
        string id,
        string kind,
        string status,
        int? statusEta = null,
        bool? kycVerified = null,
        string? moreInfoUrl = null,
        string? amountIn = null,
        string? amountInAsset = null,
        string? amountOut = null,
        string? amountOutAsset = null,
        string? amountFee = null,
        string? amountFeeAsset = null,
        string? quoteId = null,
        string? startedAt = null,
        string? completedAt = null,
        string? updatedAt = null,
        string? userActionRequiredBy = null,
        string? stellarTransactionId = null,
        string? externalTransactionId = null,
        string? message = null,
        bool? refunded = null,
        Sep24Refund? refunds = null,
        Sep24FeeDetails? feeDetails = null,
        string? from = null,
        string? to = null,
        string? depositMemo = null,
        string? depositMemoType = null,
        string? claimableBalanceId = null,
        string? withdrawAnchorAccount = null,
        string? withdrawMemo = null,
        string? withdrawMemoType = null)
    {
        Id = id;
        Kind = kind;
        Status = status;
        StatusEta = statusEta;
        KycVerified = kycVerified;
        MoreInfoUrl = moreInfoUrl;
        AmountIn = amountIn;
        AmountInAsset = amountInAsset;
        AmountOut = amountOut;
        AmountOutAsset = amountOutAsset;
        AmountFee = amountFee;
        AmountFeeAsset = amountFeeAsset;
        QuoteId = quoteId;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        UpdatedAt = updatedAt;
        UserActionRequiredBy = userActionRequiredBy;
        StellarTransactionId = stellarTransactionId;
        ExternalTransactionId = externalTransactionId;
        Message = message;
        Refunded = refunded;
        Refunds = refunds;
        FeeDetails = feeDetails;
        From = from;
        To = to;
        DepositMemo = depositMemo;
        DepositMemoType = depositMemoType;
        ClaimableBalanceId = claimableBalanceId;
        WithdrawAnchorAccount = withdrawAnchorAccount;
        WithdrawMemo = withdrawMemo;
        WithdrawMemoType = withdrawMemoType;
    }

    /// <summary>
    ///     Gets the unique, anchor-generated ID for the deposit or withdrawal.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; }

    /// <summary>
    ///     Gets the type of transaction: 'deposit' or 'withdrawal'.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; }

    /// <summary>
    ///     Gets the processing status of the deposit or withdrawal.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; }

    /// <summary>
    ///     Gets the estimated number of seconds until a status change is expected.
    /// </summary>
    [JsonPropertyName("status_eta")]
    public int? StatusEta { get; }

    /// <summary>
    ///     Gets a value indicating whether the anchor has verified the user's KYC information for this transaction.
    /// </summary>
    [JsonPropertyName("kyc_verified")]
    public bool? KycVerified { get; }

    /// <summary>
    ///     Gets a URL that is opened by wallets after the interactive flow is complete.
    ///     It can include banking information for users to start deposits, the status of the transaction,
    ///     or any other information the user might need to know about the transaction.
    /// </summary>
    [JsonPropertyName("more_info_url")]
    public string? MoreInfoUrl { get; }

    /// <summary>
    ///     Gets the amount received by anchor at start of transaction as a string with up to 7 decimals.
    ///     Excludes any fees charged before the anchor received the funds.
    /// </summary>
    [JsonPropertyName("amount_in")]
    public string? AmountIn { get; }

    /// <summary>
    ///     Gets the asset received or to be received by the Anchor.
    ///     Must be present if the deposit/withdraw was made using non-equivalent assets.
    ///     The value must be in SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("amount_in_asset")]
    public string? AmountInAsset { get; }

    /// <summary>
    ///     Gets the amount sent by anchor to user at end of transaction as a string with up to 7 decimals.
    ///     Excludes amount converted to XLM to fund account and any external fees.
    /// </summary>
    [JsonPropertyName("amount_out")]
    public string? AmountOut { get; }

    /// <summary>
    ///     Gets the asset delivered or to be delivered to the user.
    ///     Must be present if the deposit/withdraw was made using non-equivalent assets.
    ///     The value must be in SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("amount_out_asset")]
    public string? AmountOutAsset { get; }

    /// <summary>
    ///     Gets the amount of fee charged by anchor.
    /// </summary>
    [JsonPropertyName("amount_fee")]
    public string? AmountFee { get; }

    /// <summary>
    ///     Gets the asset in which fees are calculated in.
    ///     Must be present if the deposit/withdraw was made using non-equivalent assets.
    ///     The value must be in SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("amount_fee_asset")]
    public string? AmountFeeAsset { get; }

    /// <summary>
    ///     Gets the ID of the quote used when creating this transaction.
    ///     Should be present if a quote_id was included in the POST /transactions/deposit/interactive
    ///     or POST /transactions/withdraw/interactive request.
    /// </summary>
    [JsonPropertyName("quote_id")]
    public string? QuoteId { get; }

    /// <summary>
    ///     Gets the start date and time of transaction. UTC ISO 8601 string.
    /// </summary>
    [JsonPropertyName("started_at")]
    public string? StartedAt { get; }

    /// <summary>
    ///     Gets the date and time of transaction reaching completed or refunded status. UTC ISO 8601 string.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; }

    /// <summary>
    ///     Gets the date and time of transaction reaching the current status. UTC ISO 8601 string.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; }

    /// <summary>
    ///     Gets the date and time by when the user action is required.
    ///     In certain statuses, such as pending_user_transfer_start or incomplete,
    ///     anchor waits for the user action and user_action_required_by field should
    ///     be used to show the time anchors gives for the user to make an action
    ///     before transaction will automatically be moved into a different status.
    /// </summary>
    [JsonPropertyName("user_action_required_by")]
    public string? UserActionRequiredBy { get; }

    /// <summary>
    ///     Gets the transaction_id on Stellar network of the transfer that either completed the deposit or started the withdrawal.
    /// </summary>
    [JsonPropertyName("stellar_transaction_id")]
    public string? StellarTransactionId { get; }

    /// <summary>
    ///     Gets the ID of transaction on external network that either started the deposit or completed the withdrawal.
    /// </summary>
    [JsonPropertyName("external_transaction_id")]
    public string? ExternalTransactionId { get; }

    /// <summary>
    ///     Gets the human readable explanation of transaction status, if needed.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; }

    /// <summary>
    ///     Gets a value indicating whether the transaction was refunded.
    ///     True if the transaction was refunded in full. False if the transaction was partially refunded or not refunded.
    ///     Deprecated: This field is deprecated in favor of the refunds object and the refunded status.
    ///     For more details about any refunds, see the Refunds object.
    /// </summary>
    [JsonPropertyName("refunded")]
    public bool? Refunded { get; }

    /// <summary>
    ///     Gets an object describing any on-chain or off-chain refund associated with this transaction.
    ///     Contains detailed information about refund amounts, fees, and individual payment records.
    /// </summary>
    [JsonPropertyName("refunds")]
    public Sep24Refund? Refunds { get; }

    /// <summary>
    ///     Gets the description of fee charged by the anchor.
    ///     Contains the total fee amount, the asset in which fees are calculated, and an optional breakdown
    ///     detailing the individual fee components. This replaces the deprecated amount_fee field.
    ///     If quote_id is present, it should match the referenced quote's fee object.
    /// </summary>
    [JsonPropertyName("fee_details")]
    public Sep24FeeDetails? FeeDetails { get; }

    /// <summary>
    ///     Gets the source address.
    ///     In case of deposit: Sent from address, perhaps BTC, IBAN, or bank account.
    ///     In case of withdraw: Stellar address the assets were withdrawn from.
    /// </summary>
    [JsonPropertyName("from")]
    public string? From { get; }

    /// <summary>
    ///     Gets the destination address.
    ///     In case of deposit: Stellar address the deposited assets were sent to.
    ///     In case of withdraw: Sent to address (perhaps BTC, IBAN, or bank account in the case of a withdrawal,
    ///     Stellar address in the case of a deposit).
    /// </summary>
    [JsonPropertyName("to")]
    public string? To { get; }

    /// <summary>
    ///     Gets the memo (if any) used to transfer the asset to the to Stellar address.
    ///     Fields for deposit transactions.
    /// </summary>
    [JsonPropertyName("deposit_memo")]
    public string? DepositMemo { get; }

    /// <summary>
    ///     Gets the type for the deposit_memo.
    /// </summary>
    [JsonPropertyName("deposit_memo_type")]
    public string? DepositMemoType { get; }

    /// <summary>
    ///     Gets the ID of the Claimable Balance used to send the asset initially requested.
    /// </summary>
    [JsonPropertyName("claimable_balance_id")]
    public string? ClaimableBalanceId { get; }

    /// <summary>
    ///     Gets the anchor's Stellar account that the user transferred (or will transfer) their asset to.
    ///     Fields for withdraw transactions.
    /// </summary>
    [JsonPropertyName("withdraw_anchor_account")]
    public string? WithdrawAnchorAccount { get; }

    /// <summary>
    ///     Gets the memo used when the user transferred to withdraw_anchor_account.
    ///     Assigned null if the withdraw is not ready to receive payment, for example if KYC is not completed.
    /// </summary>
    [JsonPropertyName("withdraw_memo")]
    public string? WithdrawMemo { get; }

    /// <summary>
    ///     Gets the memo type for withdraw_memo.
    /// </summary>
    [JsonPropertyName("withdraw_memo_type")]
    public string? WithdrawMemoType { get; }
}

