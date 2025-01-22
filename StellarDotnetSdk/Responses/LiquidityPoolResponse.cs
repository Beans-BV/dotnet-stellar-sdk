using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class LiquidityPoolResponse : Response, IPagingToken
{
    [JsonPropertyName("id")] public LiquidityPoolId Id { get; init; }

    [JsonPropertyName("fee_bp")]
    public int FeeBp { get; init; }

    [JsonConverter(typeof(LiquidityPoolTypeEnumJsonConverter))]
    [JsonPropertyName("type")]
    public LiquidityPoolType.LiquidityPoolTypeEnum Type { get; init; }

    [JsonPropertyName("total_trustlines")]
    public string TotalTrustlines { get; init; }

    [JsonPropertyName("total_shares")]
    public string TotalShares { get; init; }

    [JsonPropertyName("reserves")]
    public Reserve[] Reserves { get; init; }

    [JsonPropertyName("_links")]
    public LiquidityPoolResponseLinks Links { get; init; }

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    public class LiquidityPoolResponseLinks
    {
        [JsonPropertyName("effects")]
        public Link Effects { get; init; }

        [JsonPropertyName("operations")]
        public Link Operations { get; init; }

        [JsonPropertyName("self")] public Link Self { get; init; }

        [JsonPropertyName("transactions")]
        public Link Transactions { get; init; }
    }
}