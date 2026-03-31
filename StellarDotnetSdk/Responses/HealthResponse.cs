using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents the health check response from a Horizon server.
///     Provides information about the operational status of the Horizon instance.
/// </summary>
public sealed class HealthResponse : Response
{
    /// <summary>
    ///     Whether the database is connected and responsive.
    /// </summary>
    [JsonPropertyName("database_connected")]
    public required bool DatabaseConnected { get; init; }

    /// <summary>
    ///     Whether the Stellar Core instance is up and running.
    /// </summary>
    [JsonPropertyName("core_up")]
    public required bool CoreUp { get; init; }

    /// <summary>
    ///     Whether the Stellar Core instance is synced with the network.
    /// </summary>
    [JsonPropertyName("core_synced")]
    public required bool CoreSynced { get; init; }
}