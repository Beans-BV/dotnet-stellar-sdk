using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a payment operation response.
///     Sends an amount in a specific asset from one account to another.
/// </summary>
public class PaymentOperationResponse : OperationResponse
{
    public override int TypeId => 1;

    /// <summary>
    ///     The amount of the asset to send.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The type of asset being sent (e.g., "native", "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The asset code (e.g., "USD", "BTC"). Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The account that issued the asset. Only present for non-native assets.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The account address that sent the payment.
    /// </summary>
    [JsonPropertyName("from")]
    public required string From { get; init; }

    /// <summary>
    ///     The muxed account representation of the sender, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed")]
    public string? FromMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the sender, if applicable.
    /// </summary>
    [JsonPropertyName("from_muxed_id")]
    public ulong? FromMuxedId { get; init; }

    /// <summary>
    ///     The account address that received the payment.
    /// </summary>
    [JsonPropertyName("to")]
    public required string To { get; init; }

    /// <summary>
    ///     The muxed account representation of the receiver, if applicable.
    /// </summary>
    [JsonPropertyName("to_muxed")]
    public string? ToMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the receiver, if applicable.
    /// </summary>
    [JsonPropertyName("to_muxed_id")]
    public ulong? ToMuxedId { get; init; }

    /// <summary>
    ///     The asset that was sent in this payment.
    /// </summary>
    [JsonIgnore]
    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}