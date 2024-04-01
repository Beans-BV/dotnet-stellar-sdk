using Newtonsoft.Json;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses;

public class LiquidityPoolResponse : Response, IPagingToken
{
    public LiquidityPoolResponse(LiquidityPoolID id, string pagingToken, int feeBp,
        LiquidityPoolType.LiquidityPoolTypeEnum type, string totalTrustlines, string totalShares, Reserve[] reserves,
        LiquidityPoolResponseLinks links)
    {
        ID = id;
        PagingToken = pagingToken;
        FeeBP = feeBp;
        Type = type;
        TotalTrustlines = totalTrustlines;
        TotalShares = totalShares;
        Reserves = reserves;
        Links = links;
    }

    [JsonProperty(PropertyName = "id")] public LiquidityPoolID ID { get; set; }

    [JsonProperty(PropertyName = "fee_bp")]
    public int FeeBP { get; set; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonProperty(PropertyName = "type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; set; }

    [JsonProperty(PropertyName = "total_trustlines")]
    public string TotalTrustlines { get; set; }

    [JsonProperty(PropertyName = "total_shares")]
    public string TotalShares { get; set; }

    [JsonProperty(PropertyName = "reserves")]
    public Reserve[] Reserves { get; set; }

    [JsonProperty(PropertyName = "_links")]
    public LiquidityPoolResponseLinks Links { get; set; }

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; set; }

    public class LiquidityPoolResponseLinks
    {
        public LiquidityPoolResponseLinks(Link effects, Link operations, Link self, Link transactions)
        {
            Effects = effects;
            Operations = operations;
            Self = self;
            Transactions = transactions;
        }

        [JsonProperty(PropertyName = "effects")]
        public Link Effects { get; set; }

        [JsonProperty(PropertyName = "operations")]
        public Link Operations { get; set; }

        [JsonProperty(PropertyName = "self")] public Link Self { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public Link Transactions { get; set; }
    }
}