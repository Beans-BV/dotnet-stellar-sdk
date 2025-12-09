using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a change_trust operation response.
///     Creates, updates, or deletes a trustline between accounts.
/// </summary>
public class ChangeTrustOperationResponse : OperationResponse
{
    public override int TypeId => 6;

    /// <summary>
    ///     The type of asset (e.g., "credit_alphanum4", "credit_alphanum12", "liquidity_pool_shares").
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The asset code. Only present for credit assets.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The account that issued the asset. Only present for credit assets.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The unique identifier of the liquidity pool. Only present for liquidity_pool_shares asset type.
    /// </summary>
    [JsonPropertyName("liquidity_pool_id")]
    public string? LiquidityPoolId { get; init; }

    /// <summary>
    ///     The limit for the trustline. A limit of "0" deletes the trustline.
    /// </summary>
    [JsonPropertyName("limit")]
    public required string Limit { get; init; }

    /// <summary>
    ///     The account being trusted (the asset issuer). Only present for credit assets.
    /// </summary>
    [JsonPropertyName("trustee")]
    public string? Trustee { get; init; }

    /// <summary>
    ///     The account creating or modifying the trustline.
    /// </summary>
    [JsonPropertyName("trustor")]
    public required string Trustor { get; init; }

    /// <summary>
    ///     The muxed account representation of the trustor, if applicable.
    /// </summary>
    [JsonPropertyName("trustor_muxed")]
    public string? TrustorMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the trustor, if applicable.
    /// </summary>
    [JsonPropertyName("trustor_muxed_id")]
    public ulong? TrustorMuxedId { get; init; }

    /// <summary>
    ///     The asset for which the trustline is being modified.
    /// </summary>
    public Asset? Asset => AssetType != "liquidity_pool_shares"
        ? Asset.CreateNonNativeAsset(AssetCode!, AssetIssuer!)
        : null;
}