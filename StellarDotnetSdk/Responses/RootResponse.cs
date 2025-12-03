using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents the root endpoint response from a Horizon server.
///     Contains version and network information about the Horizon instance.
/// </summary>
public sealed class RootResponse : Response
{
    /// <summary>
    ///     Links to related resources and endpoints.
    /// </summary>
    [JsonPropertyName("_links")]
    public required RootResponseLinks Links { get; init; }

    /// <summary>
    ///     The version of the Horizon server software.
    /// </summary>
    [JsonPropertyName("horizon_version")]
    public required string HorizonVersion { get; init; }

    /// <summary>
    ///     The version of Stellar Core connected to this Horizon instance.
    /// </summary>
    [JsonPropertyName("core_version")]
    public required string StellarCoreVersion { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger stored in Horizon's history database.
    /// </summary>
    [JsonPropertyName("history_latest_ledger")]
    public required long HistoryLatestLedger { get; init; }

    /// <summary>
    ///     An ISO 8601 formatted string of when the latest ledger was closed.
    /// </summary>
    [JsonPropertyName("history_latest_ledger_closed_at")]
    public required string HistoryLatestLedgerClosedAt { get; init; }

    /// <summary>
    ///     The sequence number of the oldest ledger stored in Horizon's history database.
    /// </summary>
    [JsonPropertyName("history_elder_ledger")]
    public required long HistoryElderLedger { get; init; }

    /// <summary>
    ///     The sequence number of the latest ledger known to Stellar Core.
    /// </summary>
    [JsonPropertyName("core_latest_ledger")]
    public required long CoreLatestLedger { get; init; }

    /// <summary>
    ///     The network passphrase that identifies which Stellar network this Horizon is connected to.
    ///     Common values are "Public Global Stellar Network ; September 2015" for mainnet
    ///     and "Test SDF Network ; September 2015" for testnet.
    /// </summary>
    [JsonPropertyName("network_passphrase")]
    public required string NetworkPassphrase { get; init; }

    /// <summary>
    ///     The current protocol version in use on the network.
    /// </summary>
    [JsonPropertyName("current_protocol_version")]
    public required int CurrentProtocolVersion { get; init; }

    /// <summary>
    ///     The maximum protocol version supported by the connected Stellar Core.
    /// </summary>
    [JsonPropertyName("core_supported_protocol_version")]
    public required int CoreSupportedProtocolVersion { get; init; }

    /// <summary>
    ///     The maximum protocol version supported by the connected Stellar.
    /// </summary>
    [JsonPropertyName("supported_protocol_version")]
    public required int SupportedProtocolVersion { get; init; }
}