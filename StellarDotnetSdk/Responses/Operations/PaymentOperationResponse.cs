using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable

/// <inheritdoc />
/// <summary>
///     Sends an amount in a specific asset to a destination account.
/// </summary>
public class PaymentOperationResponse : OperationResponse
{
    public override int TypeId => 1;

    /// <summary>
    ///     Amount of the aforementioned asset to send.
    /// </summary>
    [JsonProperty(PropertyName = "amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The asset type (USD, BTC, etc.)
    /// </summary>
    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    /// <summary>
    ///     The asset code (Alpha4, Alpha12, etc.)
    /// </summary>
    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    /// <summary>
    /// </summary>
    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    /// <summary>
    ///     Account address that is sending the payment.
    /// </summary>
    [JsonProperty(PropertyName = "from")]
    public string From { get; init; }

    [JsonProperty(PropertyName = "from_muxed")]
    public string FromMuxed { get; init; }

    [JsonProperty(PropertyName = "from_muxed_id")]
    public ulong? FromMuxedID { get; init; }

    /// <summary>
    /// </summary>
    [JsonProperty(PropertyName = "to")]
    public string To { get; init; }

    [JsonProperty(PropertyName = "to_muxed")]
    public string ToMuxed { get; init; }

    [JsonProperty(PropertyName = "to_muxed_id")]
    public ulong? ToMuxedID { get; init; }

    /// <summary>
    ///     Account address that receives the payment.
    /// </summary>
    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}