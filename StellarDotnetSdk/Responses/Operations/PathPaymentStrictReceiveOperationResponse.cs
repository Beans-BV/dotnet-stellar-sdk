using System.Collections.Generic;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable

/// <inheritdoc />
/// <summary>
///     A path payment strict receive operation represents a payment from one account to another through a path.
/// </summary>
public class PathPaymentStrictReceiveOperationResponse : OperationResponse
{
    public override int TypeId => 2;

    /// <summary>
    ///     Account address that is sending the payment.
    /// </summary>
    [JsonProperty(PropertyName = "from")]
    public string From { get; init; }

    /// <summary>
    ///     Account address that receives the payment.
    /// </summary>
    [JsonProperty(PropertyName = "to")]
    public string To { get; init; }

    /// <summary>
    ///     The destination asset code (Alpha4, Alpha12, etc.)
    /// </summary>
    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    /// <summary>
    ///     The destination asset issuer account.
    /// </summary>
    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    /// <summary>
    ///     The destination asset type. (Alpha4, Alpha12, etc.)
    /// </summary>
    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    /// <summary>
    ///     The amount of destination asset the destination account receives.
    /// </summary>
    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The source asset code.
    /// </summary>
    [JsonProperty(PropertyName = "source_asset_code")]
    public string SourceAssetCode { get; init; }

    /// <summary>
    ///     The source asset issuer account.
    /// </summary>
    [JsonProperty(PropertyName = "source_asset_issuer")]
    public string SourceAssetIssuer { get; init; }

    /// <summary>
    ///     The source asset type. (Alpha4, Alpha12, etc.)
    /// </summary>
    [JsonProperty(PropertyName = "source_asset_type")]
    public string SourceAssetType { get; init; }

    /// <summary>
    ///     The maximum send amount.
    /// </summary>
    [JsonProperty(PropertyName = "source_max")]
    public string SourceMax { get; init; }

    /// <summary>
    ///     The amount sent.
    /// </summary>
    [JsonProperty(PropertyName = "source_amount")]
    public string SourceAmount { get; init; }

    /// <summary>
    ///     Additional hops the operation went through to get to the destination asset
    /// </summary>
    [JsonProperty(PropertyName = "path")]
    public IEnumerable<Asset> Path { get; init; }

    /// <summary>
    ///     Destination Asset
    /// </summary>
    public AssetTypeCreditAlphaNum DestinationAsset => Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);

    /// <summary>
    ///     Source Asset
    /// </summary>
    public AssetTypeCreditAlphaNum SourceAsset => Asset.CreateNonNativeAsset(SourceAssetCode, SourceAssetIssuer);
}