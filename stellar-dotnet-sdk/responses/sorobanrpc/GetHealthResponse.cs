using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses.sorobanrpc;

[JsonObject]
public class GetHealthResponse
{
    /// <summary>
    ///     Health status e.g. "healthy"
    /// </summary>
    [JsonProperty("status")] public readonly string Status;

    public GetHealthResponse(string status)
    {
        Status = status;
    }
}