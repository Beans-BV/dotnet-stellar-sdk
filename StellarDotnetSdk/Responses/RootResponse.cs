using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;
#nullable disable

public class RootResponse : Response
{
    [JsonPropertyName("horizon_version")]
    public string HorizonVersion { get; init; }

    [JsonPropertyName("core_version")]
    public string StellarCoreVersion { get; init; }

    [JsonPropertyName("history_latest_ledger")]
    public int HistoryLatestLedger { get; init; }

    [JsonPropertyName("history_elder_ledger")]
    public int HistoryElderLedger { get; init; }

    [JsonPropertyName("core_latest_ledger")]
    public int CoreLatestLedger { get; init; }

    [JsonPropertyName("network_passphrase")]
    public string NetworkPassphrase { get; init; }

    [JsonPropertyName("current_protocol_version")]
    public int CurrentProtocolVersion { get; init; }

    [JsonPropertyName("core_supported_protocol_version")]
    public int CoreSupportedProtocolVersion { get; init; }
}