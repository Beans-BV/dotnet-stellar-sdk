using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Response containing anchor capabilities and supported operations.
///     This response provides comprehensive information about what deposit and
///     withdrawal operations an anchor supports, including supported assets, fee
///     structures, transaction endpoints, and feature flags.
/// </summary>
public sealed class InfoResponse : Response
{
    /// <summary>
    ///     Map of assets supporting standard deposits with their configurations.
    /// </summary>
    [JsonPropertyName("deposit")]
    public Dictionary<string, DepositAsset>? DepositAssets { get; init; }

    /// <summary>
    ///     Map of assets supporting deposits with conversion.
    /// </summary>
    [JsonPropertyName("deposit-exchange")]
    public Dictionary<string, DepositExchangeAsset>? DepositExchangeAssets { get; init; }

    /// <summary>
    ///     Map of assets supporting standard withdrawals with their configurations.
    /// </summary>
    [JsonPropertyName("withdraw")]
    public Dictionary<string, WithdrawAsset>? WithdrawAssets { get; init; }

    /// <summary>
    ///     Map of assets supporting withdrawals with conversion.
    /// </summary>
    [JsonPropertyName("withdraw-exchange")]
    public Dictionary<string, WithdrawExchangeAsset>? WithdrawExchangeAssets { get; init; }

    /// <summary>
    ///     Configuration for the /fee endpoint.
    /// </summary>
    [JsonPropertyName("fee")]
    public AnchorFeeInfo? FeeInfo { get; init; }

    /// <summary>
    ///     Configuration for the /transactions endpoint.
    /// </summary>
    [JsonPropertyName("transactions")]
    public AnchorTransactionsInfo? TransactionsInfo { get; init; }

    /// <summary>
    ///     Configuration for the /transaction endpoint.
    /// </summary>
    [JsonPropertyName("transaction")]
    public AnchorTransactionInfo? TransactionInfo { get; init; }

    /// <summary>
    ///     Flags indicating supported anchor features.
    /// </summary>
    [JsonPropertyName("features")]
    public AnchorFeatureFlags? FeatureFlags { get; init; }
}