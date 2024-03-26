using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

public class PathResponseLinks
{
    public PathResponseLinks(Link<PathResponse> self)
    {
        Self = self;
    }

    [JsonProperty(PropertyName = "self")] public Link<PathResponse> Self { get; }
}