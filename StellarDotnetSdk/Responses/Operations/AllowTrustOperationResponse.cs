using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;
#nullable disable

/// <inheritdoc />
/// <summary>
///     Represents AllowTrust operation response.
/// </summary>
public class AllowTrustOperationResponse : OperationResponse
{
    public override int TypeId => 7;

    /// <summary>
    ///     Trustor account.
    /// </summary>
    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; init; }

    /// <summary>
    ///     Trustee account.
    /// </summary>
    [JsonProperty(PropertyName = "trustee")]
    public string Trustee { get; init; }

    /// <summary>
    ///     Trustee account.
    /// </summary>
    [JsonProperty(PropertyName = "trustee_muxed")]
    public string TrusteeMuxed { get; init; }

    /// <summary>
    ///     Trustee account.
    /// </summary>
    [JsonProperty(PropertyName = "trustee_muxed_id")]
    public ulong? TrusteeMuxedID { get; init; }

    /// <summary>
    ///     Asset type (native / alphanum4 / alphanum12)
    /// </summary>
    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    /// <summary>
    ///     Asset code.
    /// </summary>
    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    /// <summary>
    ///     Asset issuer.
    /// </summary>
    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    /// <summary>
    ///     true when allowing trust, false when revoking trust
    /// </summary>
    [JsonProperty(PropertyName = "authorize")]
    public bool Authorize { get; init; }

    /// <summary>
    ///     The asset to allow trust.
    /// </summary>
    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}