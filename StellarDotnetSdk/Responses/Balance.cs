using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents an account's balance in a specific asset or liquidity pool shares.
///     An account can hold multiple balances, one for each trusted asset.
/// </summary>
public sealed class Balance
{
    private const string LiquidityPoolSharesAssetType = "liquidity_pool_shares";

    /// <summary>
    ///     The type of asset. Can be "native" (XLM), "credit_alphanum4", "credit_alphanum12",
    ///     or "liquidity_pool_shares".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The maximum amount of this asset that this account is willing to accept.
    ///     For native XLM, this is not applicable. Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("limit")]
    public string? Limit { get; init; }

    /// <summary>
    ///     The current balance of this asset held by the account.
    ///     Represented as a string to preserve precision (up to 7 decimal places).
    /// </summary>
    [JsonPropertyName("balance")]
    public required string BalanceString { get; init; }

    /// <summary>
    ///     The 4 or 12 character code for non-native assets. Null for native XLM or liquidity pool shares.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The account ID of the asset issuer. Null for native XLM or liquidity pool shares.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The asset object representing this balance. Null for liquidity pool shares.
    /// </summary>
    [JsonIgnore]
    public Asset? Asset => AssetType != LiquidityPoolSharesAssetType
        ? Asset.Create(AssetType, AssetCode, AssetIssuer)
        : null;

    /// <summary>
    ///     The amount of this asset reserved for pending buy offers.
    ///     Represented as a string to preserve precision.
    ///     Null for liquidity pool shares.
    /// </summary>
    [JsonPropertyName("buying_liabilities")]
    public string? BuyingLiabilities { get; init; }

    /// <summary>
    ///     The amount of this asset reserved for pending sell offers.
    ///     Represented as a string to preserve precision.
    ///     Null for liquidity pool shares.
    /// </summary>
    [JsonPropertyName("selling_liabilities")]
    public string? SellingLiabilities { get; init; }

    /// <summary>
    ///     Whether the account is authorized to transact with this asset.
    ///     Only applicable for assets that require authorization.
    ///     For native XLM, this is not applicable.
    /// </summary>
    [JsonPropertyName("is_authorized")]
    public bool? IsAuthorized { get; init; }

    /// <summary>
    ///     Whether the account is authorized to maintain liabilities for this asset.
    ///     When true, the account can hold the asset but cannot send or receive it.
    ///     For native XLM, this is not applicable.
    /// </summary>
    [JsonPropertyName("is_authorized_to_maintain_liabilities")]
    public bool? IsAuthorizedToMaintainLiabilities { get; init; }

    /// <summary>
    ///     The unique identifier of the liquidity pool, if this balance represents liquidity pool shares.
    ///     Null for regular asset balances.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public string? LiquidityPoolId { get; init; }

    /// <summary>
    ///     The sequence number of the last ledger in which this balance was modified.
    ///     For native XLM, this is not applicable.
    /// </summary>
    [JsonPropertyName("last_modified_ledger")]
    public long? LastModifiedLedger { get; init; }
}