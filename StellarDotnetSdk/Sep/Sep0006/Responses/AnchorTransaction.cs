using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Represents an anchor transaction.
/// </summary>
public sealed class AnchorTransaction : Response
{
    /// <summary>
    ///     Unique, anchor-generated id for the deposit/withdrawal.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    ///     deposit, deposit-exchange, withdrawal or withdrawal-exchange.
    /// </summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>
    ///     Processing status of deposit/withdrawal.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    ///     Estimated number of seconds until a status change is expected.
    /// </summary>
    [JsonPropertyName("status_eta")]
    public int? StatusEta { get; init; }

    /// <summary>
    ///     A URL the user can visit if they want more information
    ///     about their account / status.
    /// </summary>
    [JsonPropertyName("more_info_url")]
    public string? MoreInfoUrl { get; init; }

    /// <summary>
    ///     Amount received by anchor at start of transaction as a
    ///     string with up to 7 decimals. Excludes any fees charged before the
    ///     anchor received the funds. Should be equals to quote.sell_asset if
    ///     a quote_id was used.
    /// </summary>
    [JsonPropertyName("amount_in")]
    public decimal? AmountIn { get; init; }

    /// <summary>
    ///     The asset received or to be received by the Anchor.
    ///     Must be present if the deposit/withdraw was made using quotes.
    ///     The value must be in SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("amount_in_asset")]
    public string? AmountInAsset { get; init; }

    /// <summary>
    ///     Amount sent by anchor to user at end of transaction as
    ///     a string with up to 7 decimals. Excludes amount converted to XLM to
    ///     fund account and any external fees. Should be equals to quote.buy_asset
    ///     if a quote_id was used.
    /// </summary>
    [JsonPropertyName("amount_out")]
    public decimal? AmountOut { get; init; }

    /// <summary>
    ///     The asset delivered or to be delivered to the user.
    ///     Must be present if the deposit/withdraw was made using quotes.
    ///     The value must be in SEP-38 Asset Identification Format.
    /// </summary>
    [JsonPropertyName("amount_out_asset")]
    public string? AmountOutAsset { get; init; }

    /// <summary>
    ///     (Deprecated, optional) Amount of fee charged by anchor.
    ///     Should be equals to quote.fee.total if a quote_id was used.
    /// </summary>
    [JsonPropertyName("amount_fee")]
    public decimal? AmountFee { get; init; }

    /// <summary>
    ///     (Deprecated, optional) The asset in which fees are calculated in.
    ///     Must be present if the deposit/withdraw was made using quotes.
    ///     The value must be in SEP-38 Asset Identification Format.
    ///     Should be equals to quote.fee.asset if a quote_id was used.
    /// </summary>
    [JsonPropertyName("amount_fee_asset")]
    public string? AmountFeeAsset { get; init; }

    /// <summary>
    ///     Description of fee charged by the anchor.
    ///     If quote_id is present, it should match the referenced quote's fee object.
    /// </summary>
    [JsonPropertyName("fee_details")]
    public FeeDetails? FeeDetails { get; init; }

    /// <summary>
    ///     The ID of the quote used to create this transaction.
    ///     Should be present if a quote_id was included in the POST /transactions
    ///     request.
    /// </summary>
    [JsonPropertyName("quote_id")]
    public string? QuoteId { get; init; }

    /// <summary>
    ///     Sent from address (perhaps BTC, IBAN, or bank account in
    ///     the case of a deposit, Stellar address in the case of a withdrawal).
    /// </summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }

    /// <summary>
    ///     Sent to address (perhaps BTC, IBAN, or bank account in
    ///     the case of a withdrawal, Stellar address in the case of a deposit).
    /// </summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>
    ///     Extra information for the external account involved.
    ///     It could be a bank routing number, BIC, or store number for example.
    /// </summary>
    [JsonPropertyName("external_extra")]
    public string? ExternalExtra { get; init; }

    /// <summary>
    ///     Text version of external_extra.
    ///     This is the name of the bank or store
    /// </summary>
    [JsonPropertyName("external_extra_text")]
    public string? ExternalExtraText { get; init; }

    /// <summary>
    ///     If this is a deposit, this is the memo (if any)
    ///     used to transfer the asset to the to Stellar address
    /// </summary>
    [JsonPropertyName("deposit_memo")]
    public string? DepositMemo { get; init; }

    /// <summary>
    ///     Type for the depositMemo.
    /// </summary>
    [JsonPropertyName("deposit_memo_type")]
    public string? DepositMemoType { get; init; }

    /// <summary>
    ///     If this is a withdrawal, this is the anchor's Stellar account
    ///     that the user transferred (or will transfer) their issued asset to.
    /// </summary>
    [JsonPropertyName("withdraw_anchor_account")]
    public string? WithdrawAnchorAccount { get; init; }

    /// <summary>
    ///     Memo used when the user transferred to withdrawAnchorAccount.
    /// </summary>
    [JsonPropertyName("withdraw_memo")]
    public string? WithdrawMemo { get; init; }

    /// <summary>
    ///     Memo type for withdrawMemo.
    /// </summary>
    [JsonPropertyName("withdraw_memo_type")]
    public string? WithdrawMemoType { get; init; }

    /// <summary>
    ///     Start date and time of transaction - UTC ISO 8601 string.
    /// </summary>
    [JsonPropertyName("started_at")]
    public string? StartedAt { get; init; }

    /// <summary>
    ///     The date and time of transaction reaching the current status.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; init; }

    /// <summary>
    ///     Completion date and time of transaction - UTC ISO 8601 string.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; init; }

    /// <summary>
    ///     The date and time by when the user action is required.
    ///     In certain statuses, such as pending_user_transfer_start or incomplete,
    ///     anchor waits for the user action and user_action_required_by field should
    ///     be used to show the time anchors gives for the user to make an action
    ///     before transaction will automatically be moved into a different status.
    /// </summary>
    [JsonPropertyName("user_action_required_by")]
    public string? UserActionRequiredBy { get; init; }

    /// <summary>
    ///     transaction_id on Stellar network of the transfer that either
    ///     completed the deposit or started the withdrawal.
    /// </summary>
    [JsonPropertyName("stellar_transaction_id")]
    public string? StellarTransactionId { get; init; }

    /// <summary>
    ///     ID of transaction on external network that either started
    ///     the deposit or completed the withdrawal.
    /// </summary>
    [JsonPropertyName("external_transaction_id")]
    public string? ExternalTransactionId { get; init; }

    /// <summary>
    ///     Human readable explanation of transaction status, if needed.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    ///     (Deprecated, optional) This field is deprecated in favor of the refunds
    ///     object. True if the transaction was refunded in full. False if the
    ///     transaction was partially refunded or not refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public bool? Refunded { get; init; }

    /// <summary>
    ///     An object describing any on or off-chain refund associated
    ///     with this transaction.
    /// </summary>
    [JsonPropertyName("refunds")]
    public TransactionRefunds? Refunds { get; init; }

    /// <summary>
    ///     A human-readable message indicating any errors that require
    ///     updated information from the user.
    /// </summary>
    [JsonPropertyName("required_info_message")]
    public string? RequiredInfoMessage { get; init; }

    /// <summary>
    ///     A set of fields that require update from the user described in
    ///     the same format as /info. This field is only relevant when status is
    ///     pending_transaction_info_update.
    /// </summary>
    [JsonPropertyName("required_info_updates")]
    public Dictionary<string, AnchorField>? RequiredInfoUpdates { get; init; }

    /// <summary>
    ///     JSON object containing the SEP-9 financial account fields that
    ///     describe how to complete the off-chain deposit in the same format as
    ///     the /deposit response. This field should be present if the instructions
    ///     were provided in the /deposit response or if it could not have been
    ///     previously provided synchronously.
    /// </summary>
    [JsonPropertyName("instructions")]
    public Dictionary<string, DepositInstruction>? Instructions { get; init; }

    /// <summary>
    ///     ID of the Claimable Balance used to send the asset initially
    ///     requested. Only relevant for deposit transactions.
    /// </summary>
    [JsonPropertyName("claimable_balance_id")]
    public string? ClaimableBalanceId { get; init; }
}

