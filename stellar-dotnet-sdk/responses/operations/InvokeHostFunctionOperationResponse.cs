using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.operations;

public class InvokeHostFunctionOperationResponse : OperationResponse
{
    [JsonProperty(PropertyName = "address")]
    public string Address;

    [JsonProperty(PropertyName = "asset_balance_changes")]
    public AssetContractBalanceChange[] assetBalanceChanges;

    [JsonProperty(PropertyName = "function")]
    public string Function;

    [JsonProperty(PropertyName = "parameters")]
    public HostFunctionParameter[] Parameters;

    [JsonProperty(PropertyName = "salt")] public string Salt;

    public class HostFunctionParameter
    {
        [JsonProperty(PropertyName = "type")] public string Type;

        [JsonProperty(PropertyName = "value")] public string Value;
    }

    public class AssetContractBalanceChange
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount;

        [JsonProperty(PropertyName = "asset_code")]
        public string AssetCode;

        [JsonProperty(PropertyName = "asset_issuer")]
        public string AssetIssuer;

        [JsonProperty(PropertyName = "asset_type")]
        public string AssetType;

        [JsonProperty(PropertyName = "from")] public string From;

        [JsonProperty(PropertyName = "to")] public string To;

        [JsonProperty(PropertyName = "type")] public string Type;
    }
}