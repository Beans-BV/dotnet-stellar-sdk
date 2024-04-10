using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable
/// <inheritdoc />
/// <summary>
///     Represents SetTrustlineFlags operation response.
/// </summary>
public class SetTrustlineFlagsOperationResponse : OperationResponse
{
    public override int TypeId => 21;

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
    ///     Trustor account.
    /// </summary>
    [JsonProperty(PropertyName = "trustor")]
    public string Trustor { get; init; }

    /// <summary>
    ///     Indicates which flags to clear. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     adds onto the
    ///     existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonProperty(PropertyName = "clear_flags_s")]
    public string[] ClearFlags { get; init; }

    /// <summary>
    ///     Indicates which flags to set. For details about the flags, please refer to the accounts doc. The bit mask integer
    ///     adds onto the
    ///     existing flags of the account. This allows for setting specific bits without knowledge of existing flags.
    /// </summary>
    [JsonProperty(PropertyName = "set_flags_s")]
    public string[] SetFlags { get; init; }

    /// <summary>
    ///     Asset representation (Using the values of the other fields)
    /// </summary>
    public AssetTypeCreditAlphaNum Asset => Assets.Asset.CreateNonNativeAsset(AssetCode, AssetIssuer);
}