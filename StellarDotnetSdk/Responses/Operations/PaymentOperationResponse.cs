using System.Text.Json.Serialization;
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
    [JsonPropertyName("amount")]
    public string Amount { get; init; }

    /// <summary>
    ///     The asset type (USD, BTC, etc.)
    /// </summary>
    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    /// <summary>
    ///     The asset code (Alpha4, Alpha12, etc.)
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    /// <summary>
    ///     Account address that is sending the payment.
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; init; }

    [JsonPropertyName("from_muxed")]
    public string FromMuxed { get; init; }

    [JsonPropertyName("from_muxed_id")]
    public ulong? FromMuxedId { get; init; }

    /// <summary>
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; init; }

    [JsonPropertyName("to_muxed")]
    public string ToMuxed { get; init; }

    [JsonPropertyName("to_muxed_id")]
    public ulong? ToMuxedId { get; init; }

    /// <summary>
    ///     Account address that receives the payment.
    /// </summary>
    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}