using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the contract_debited effect response.
///     This effect occurs when a Soroban smart contract sends some currency
///     from SAC events involving transfers, mints, and burns.
/// </summary>
public sealed class ContractDebitedEffectResponse : EffectResponse
{
    /// <summary>
    ///     The numeric type identifier for the contract_debited effect.
    /// </summary>
    public override int TypeId => 97;

    /// <summary>
    ///     The amount debited from the contract.
    /// </summary>
    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    /// <summary>
    ///     The code of the debited asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The issuer of the debited asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The type of asset debited: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The contract address that was debited.
    /// </summary>
    [JsonPropertyName("contract")]
    public required string Contract { get; init; }

    /// <summary>
    ///     The debited asset.
    /// </summary>
    [JsonIgnore]
    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
}