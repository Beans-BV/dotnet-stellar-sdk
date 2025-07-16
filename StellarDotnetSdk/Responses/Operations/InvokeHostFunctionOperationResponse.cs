using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Operations;

#nullable disable

public class InvokeHostFunctionOperationResponse : OperationResponse
{
    public override int TypeId => 24;

    [JsonProperty(PropertyName = "address")]
    public string Address { get; init; }

    [JsonProperty(PropertyName = "asset_balance_changes")]
    public AssetContractBalanceChange[] AssetBalanceChanges { get; init; }

    [JsonProperty(PropertyName = "function")]
    public string Function { get; init; }

    [JsonProperty(PropertyName = "parameters")]
    public HostFunctionParameter[] Parameters { get; init; }

    [JsonProperty(PropertyName = "salt")] public string Salt { get; init; }

    public class HostFunctionParameter
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; init; }

        [JsonProperty(PropertyName = "value")] public string Value { get; init; }
    }

    public class AssetContractBalanceChange
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; init; }

        [JsonProperty(PropertyName = "asset_code")]
        public string AssetCode { get; init; }

        [JsonProperty(PropertyName = "asset_issuer")]
        public string AssetIssuer { get; init; }

        [JsonProperty(PropertyName = "asset_type")]
        public string AssetType { get; init; }
        public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);
        
        [JsonProperty(PropertyName = "destination_muxed_id")]
        public string DestinationMuxedId { get; private set; }

        [JsonProperty(PropertyName = "destination_muxed_id_type")]
        public MuxedIdType? DestinationMuxedIdType { get; private set; }

        [JsonProperty(PropertyName = "from")] public string From { get; init; }

        [JsonProperty(PropertyName = "to")] public string To { get; init; }

        [JsonProperty(PropertyName = "type")] public string Type { get; init; }

        public enum MuxedIdType
        {
            STRING,
            UINT64,
            BYTES,
        }
    }
}