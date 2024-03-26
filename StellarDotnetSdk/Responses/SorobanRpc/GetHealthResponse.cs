using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses.SorobanRpc;

[JsonObject]
public class GetHealthResponse
{
    public GetHealthResponse(string status)
    {
        Status = status;
    }

    /// <summary>
    ///     Health status e.g. "healthy"
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; }
}