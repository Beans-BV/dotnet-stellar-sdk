using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class RootResponse : Response
{
    [JsonProperty(PropertyName = "horizon_version")]
    public string HorizonVersion { get; init; }

    [JsonProperty(PropertyName = "core_version")]
    public string StellarCoreVersion { get; init; }

    [JsonProperty(PropertyName = "history_latest_ledger")]
    public int HistoryLatestLedger { get; init; }

    [JsonProperty(PropertyName = "history_elder_ledger")]
    public int HistoryElderLedger { get; init; }

    [JsonProperty(PropertyName = "core_latest_ledger")]
    public int CoreLatestLedger { get; init; }

    [JsonProperty(PropertyName = "network_passphrase")]
    public string NetworkPassphrase { get; init; }

    [JsonProperty(PropertyName = "current_protocol_version")]
    public int CurrentProtocolVersion { get; init; }

    [JsonProperty(PropertyName = "core_supported_protocol_version")]
    public int CoreSupportedProtocolVersion { get; init; }
}