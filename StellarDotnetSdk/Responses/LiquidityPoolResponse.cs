using Newtonsoft.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class LiquidityPoolResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "id")] public LiquidityPoolID Id { get; init; }

    [JsonProperty(PropertyName = "fee_bp")]
    public int FeeBp { get; init; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonProperty(PropertyName = "type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; init; }

    [JsonProperty(PropertyName = "total_trustlines")]
    public string TotalTrustlines { get; init; }

    [JsonProperty(PropertyName = "total_shares")]
    public string TotalShares { get; init; }

    [JsonProperty(PropertyName = "reserves")]
    public Reserve[] Reserves { get; init; }

    [JsonProperty(PropertyName = "_links")]
    public LiquidityPoolResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    public class LiquidityPoolResponseLinks
    {
        [JsonProperty(PropertyName = "effects")]
        public Link Effects { get; init; }

        [JsonProperty(PropertyName = "operations")]
        public Link Operations { get; init; }

        [JsonProperty(PropertyName = "self")] public Link Self { get; init; }

        [JsonProperty(PropertyName = "transactions")]
        public Link Transactions { get; init; }
    }
}