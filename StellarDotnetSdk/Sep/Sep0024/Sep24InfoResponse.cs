using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Response from the /info endpoint containing anchor capabilities.
///     This response provides comprehensive information about which assets the anchor supports
///     for deposits and withdrawals, fee structures, and available features.
///     Authentication: Not required.
/// </summary>
public class Sep24InfoResponse : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24InfoResponse" /> class.
    /// </summary>
    /// <param name="depositAssets">Map of asset codes to deposit configuration.</param>
    /// <param name="withdrawAssets">Map of asset codes to withdrawal configuration.</param>
    /// <param name="feeEndpointInfo">Information about the /fee endpoint if available.</param>
    /// <param name="featureFlags">Optional feature flags indicating advanced capabilities.</param>
    [JsonConstructor]
    public Sep24InfoResponse(
        Dictionary<string, Sep24DepositAsset>? depositAssets = null,
        Dictionary<string, Sep24WithdrawAsset>? withdrawAssets = null,
        Sep24FeeEndpointInfo? feeEndpointInfo = null,
        Sep24FeatureFlags? featureFlags = null)
    {
        DepositAssets = depositAssets;
        WithdrawAssets = withdrawAssets;
        FeeEndpointInfo = feeEndpointInfo;
        FeatureFlags = featureFlags;
    }

    /// <summary>
    ///     Gets the map of asset codes to deposit configuration.
    ///     Keys are asset codes (e.g., 'USD', 'BTC'), values contain deposit details.
    /// </summary>
    [JsonPropertyName("deposit")]
    public Dictionary<string, Sep24DepositAsset>? DepositAssets { get; }

    /// <summary>
    ///     Gets the map of asset codes to withdrawal configuration.
    ///     Keys are asset codes (e.g., 'USD', 'BTC'), values contain withdrawal details.
    /// </summary>
    [JsonPropertyName("withdraw")]
    public Dictionary<string, Sep24WithdrawAsset>? WithdrawAssets { get; }

    /// <summary>
    ///     Gets information about the /fee endpoint if available.
    /// </summary>
    [JsonPropertyName("fee")]
    public Sep24FeeEndpointInfo? FeeEndpointInfo { get; }

    /// <summary>
    ///     Gets optional feature flags indicating advanced capabilities.
    /// </summary>
    [JsonPropertyName("features")]
    public Sep24FeatureFlags? FeatureFlags { get; }
}

