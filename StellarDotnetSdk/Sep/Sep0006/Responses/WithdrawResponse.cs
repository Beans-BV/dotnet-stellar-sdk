using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Represents a transfer service withdraw response.
/// </summary>
public sealed class WithdrawResponse : Response
{
    /// <summary>
    ///     The account the user should send its token back to.
    ///     This field can be omitted if the anchor cannot provide this information
    ///     at the time of the request.
    /// </summary>
    [JsonPropertyName("account_id")]
    public string? AccountId { get; init; }

    /// <summary>
    ///     Type of memo to attach to transaction, one of text, id or hash.
    /// </summary>
    [JsonPropertyName("memo_type")]
    public string? MemoType { get; init; }

    /// <summary>
    ///     Value of memo to attach to transaction, for hash this should
    ///     be base64-encoded. The anchor should use this memo to match the Stellar
    ///     transaction with the database entry associated created to represent it.
    /// </summary>
    [JsonPropertyName("memo")]
    public string? Memo { get; init; }

    /// <summary>
    ///     The anchor's ID for this withdrawal. The wallet will use this
    ///     ID to query the /transaction endpoint to check status of the request.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    ///     Estimate of how long the withdrawal will take to credit in seconds.
    /// </summary>
    [JsonPropertyName("eta")]
    public int? Eta { get; init; }

    /// <summary>
    ///     Minimum amount of an asset that a user can withdraw.
    /// </summary>
    [JsonPropertyName("min_amount")]
    public decimal? MinAmount { get; init; }

    /// <summary>
    ///     Maximum amount of asset that a user can withdraw.
    /// </summary>
    [JsonPropertyName("max_amount")]
    public decimal? MaxAmount { get; init; }

    /// <summary>
    ///     If there is a fee for withdraw. In units of the withdrawn asset.
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; init; }

    /// <summary>
    ///     If there is a percent fee for withdraw.
    /// </summary>
    [JsonPropertyName("fee_percent")]
    public decimal? FeePercent { get; init; }

    /// <summary>
    ///     Any additional data needed as an input for this withdraw,
    ///     example: Bank Name.
    /// </summary>
    [JsonPropertyName("extra_info")]
    public ExtraInfo? ExtraInfo { get; init; }
}