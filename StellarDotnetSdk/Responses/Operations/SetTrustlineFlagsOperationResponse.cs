using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents a set_trustline_flags operation response.
///     Sets flags on a trustline, controlling authorization states for an asset.
/// </summary>
public class SetTrustlineFlagsOperationResponse : OperationResponse
{
    public override int TypeId => 21;

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
    ///     The account whose trustline flags are being modified.
    /// </summary>
    [JsonPropertyName("trustor")]
    public required string Trustor { get; init; }

    /// <summary>
    ///     Array of flag names to clear (e.g., ["authorized", "authorized_to_maintain_liabilities"]).
    /// </summary>
    [JsonPropertyName("clear_flags_s")]
    public required string[] ClearFlags { get; init; }

    /// <summary>
    ///     Array of flag names to set (e.g., ["authorized", "clawback_enabled"]).
    /// </summary>
    [JsonPropertyName("set_flags_s")]
    public required string[] SetFlags { get; init; }

    /// <summary>
    ///     The asset for which trustline flags are being modified.
    /// </summary>
    [JsonIgnore]
    public Asset Asset => Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}