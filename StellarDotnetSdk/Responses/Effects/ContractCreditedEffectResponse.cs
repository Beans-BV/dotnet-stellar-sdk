using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Represents the contract_credited effect response.
///     This effect occurs when a smart contract receives a payment.
/// </summary>
public sealed class ContractCreditedEffectResponse : EffectResponse
{
    // TODO: Find out which TypeId and add tests
    // public override int TypeId => ;

    /// <summary>
    ///     The amount credited to the contract.
    /// </summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    /// <summary>
    ///     The code of the credited asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The issuer of the credited asset. Null for native XLM.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     The type of asset credited: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public string? AssetType { get; init; }

    /// <summary>
    ///     The contract address that was credited.
    /// </summary>
    [JsonPropertyName("contract")]
    public string? Contract { get; init; }

    /// <summary>
    ///     The credited asset.
    /// </summary>
    public Asset? Asset => AssetType != null ? Asset.Create(AssetType, AssetCode, AssetIssuer) : null;
}