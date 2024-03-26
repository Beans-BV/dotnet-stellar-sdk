using Newtonsoft.Json;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

public class TradeResponseLinks
{
    [JsonProperty(PropertyName = "base")] public Link<AssetResponse> Base;

    [JsonProperty(PropertyName = "counter")]
    public Link<AssetResponse> Counter;

    [JsonProperty(PropertyName = "operation")]
    public Link<OperationResponse> Operation;

    public TradeResponseLinks(Link<AssetResponse> baseLink, Link<AssetResponse> counterLink,
        Link<OperationResponse> operationLink)
    {
        Base = baseLink;
        Counter = counterLink;
        Operation = operationLink;
    }
}