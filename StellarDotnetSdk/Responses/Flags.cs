using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents account flags.
/// </summary>
public class Flags
{
    /// <summary>
    ///     This account must approve anyone who wants to hold its asset.
    /// </summary>
    [JsonPropertyName("auth_required")]
    public bool AuthRequired { get; init; }

    /// <summary>
    ///     This account can set the authorize flag of an existing trustline to freeze the assets held by an asset holder.
    /// </summary>
    [JsonPropertyName("auth_revocable")]
    public bool AuthRevocable { get; init; }

    /// <summary>
    ///     This account cannot change any of the authorization flags.
    /// </summary>
    [JsonPropertyName("auth_immutable")]
    public bool AuthImmutable { get; init; }

    /// <summary>
    ///     This account can unilaterally take away any portion of its issued asset(s) from any asset holders.
    /// </summary>
    [JsonPropertyName("auth_clawback_enabled")]
    public bool AuthClawback { get; init; }
}