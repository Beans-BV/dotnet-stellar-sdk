using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an allow_trust operation response (deprecated operation).
///     Allows another account to hold an asset issued by the source account.
/// </summary>
/// <remarks>
///     This operation is deprecated. Use <see cref="SetTrustlineFlagsOperationResponse" /> instead.
/// </remarks>
[Obsolete("This operation is deprecated as of Protocol 17. Prefer SetTrustLineFlags instead.")]
public class AllowTrustOperationResponse : OperationResponse
{
    public override int TypeId => 7;

    /// <summary>
    ///     The account that is being authorized or deauthorized.
    /// </summary>
    [JsonPropertyName("trustor")]
    public required string Trustor { get; init; }

    /// <summary>
    ///     The asset issuer account (typically the source account).
    /// </summary>
    [JsonPropertyName("trustee")]
    public required string Trustee { get; init; }

    /// <summary>
    ///     The muxed account representation of the trustee, if applicable.
    /// </summary>
    [JsonPropertyName("trustee_muxed")]
    public string? TrusteeMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the trustee, if applicable.
    /// </summary>
    [JsonPropertyName("trustee_muxed_id")]
    public ulong? TrusteeMuxedId { get; init; }

    /// <summary>
    ///     The type of asset (e.g., "credit_alphanum4", "credit_alphanum12").
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The asset code.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public required string AssetCode { get; init; }

    /// <summary>
    ///     The asset issuer account.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public required string AssetIssuer { get; init; }

    /// <summary>
    ///     Indicates whether the trustline is being authorized (true) or deauthorized (false).
    /// </summary>
    [JsonPropertyName("authorize")]
    public bool Authorize { get; init; }

    /// <summary>
    ///     The asset for which trust is being allowed or revoked.
    /// </summary>
    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}