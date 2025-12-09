using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

/// <summary>
///     Represents an invoke_host_function operation response.
///     Invokes a Soroban smart contract function on the Stellar network.
/// </summary>
public class InvokeHostFunctionOperationResponse : OperationResponse
{
    public override int TypeId => 24;

    /// <summary>
    ///     The contract address being invoked. Only present for contract invocations.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>
    ///     Array of asset balance changes that occurred during contract execution.
    /// </summary>
    [JsonPropertyName("asset_balance_changes")]
    public AssetContractBalanceChange[]? AssetBalanceChanges { get; init; }

    /// <summary>
    ///     The host function type being invoked (e.g., "HostFunctionTypeHostFunctionTypeInvokeContract").
    /// </summary>
    [JsonPropertyName("function")]
    public required string Function { get; init; }

    /// <summary>
    ///     Array of parameters passed to the host function.
    /// </summary>
    [JsonPropertyName("parameters")]
    public HostFunctionParameter[]? Parameters { get; init; }

    /// <summary>
    ///     The salt value used for contract deployment. Only present for contract deployments.
    /// </summary>
    [JsonPropertyName("salt")]
    public string? Salt { get; init; }

    /// <summary>
    ///     Represents a parameter passed to a host function.
    /// </summary>
    public class HostFunctionParameter
    {
        /// <summary>
        ///     The type of the parameter (e.g., "Address", "Sym", "I128").
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        /// <summary>
        ///     The base64-encoded value of the parameter.
        /// </summary>
        [JsonPropertyName("value")]
        public required string Value { get; init; }
    }

    /// <summary>
    ///     Represents an asset balance change that occurred during contract execution.
    /// </summary>
    public class AssetContractBalanceChange
    {
        /// <summary>
        ///     The type of muxed ID for the destination account.
        /// </summary>
        public enum MuxedIdType
        {
            /// <summary>String representation.</summary>
            STRING,

            /// <summary>64-bit unsigned integer.</summary>
            UINT64,

            /// <summary>Binary bytes.</summary>
            BYTES,
        }

        /// <summary>
        ///     The amount of the asset that was transferred.
        /// </summary>
        [JsonPropertyName("amount")]
        public required string Amount { get; init; }

        /// <summary>
        ///     The asset code. Only present for non-native assets.
        /// </summary>
        [JsonPropertyName("asset_code")]
        public string? AssetCode { get; init; }

        /// <summary>
        ///     The asset issuer account. Only present for non-native assets.
        /// </summary>
        [JsonPropertyName("asset_issuer")]
        public string? AssetIssuer { get; init; }

        /// <summary>
        ///     The type of asset (e.g., "native", "credit_alphanum4", "credit_alphanum12").
        /// </summary>
        [JsonPropertyName("asset_type")]
        public required string AssetType { get; init; }

        /// <summary>
        ///     The asset that was transferred.
        /// </summary>
        [JsonIgnore]
        public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);

        /// <summary>
        ///     The muxed ID of the destination account, if applicable.
        /// </summary>
        [JsonPropertyName("destination_muxed_id")]
        public string? DestinationMuxedId { get; init; }

        /// <summary>
        ///     The type of the destination muxed ID, if applicable.
        /// </summary>
        [JsonPropertyName("destination_muxed_id_type")]
        public MuxedIdType? DestinationMuxedIdType { get; init; }

        /// <summary>
        ///     The account address that sent the asset.
        /// </summary>
        [JsonPropertyName("from")]
        public required string From { get; init; }

        /// <summary>
        ///     The account address that received the asset.
        /// </summary>
        [JsonPropertyName("to")]
        public required string To { get; init; }

        /// <summary>
        ///     The type of balance change (e.g., "transfer", "mint", "burn").
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; init; }
    }
}