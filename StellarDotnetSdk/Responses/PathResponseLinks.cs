using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;

public class PathResponseLinks
{
    public PathResponseLinks(Link<PathResponse> self)
    {
        Self = self;
    }

    [JsonProperty(PropertyName = "self")] public Link<PathResponse> Self { get; }
}