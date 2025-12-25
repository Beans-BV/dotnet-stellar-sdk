using System.Collections.Generic;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0006.Responses;

/// <summary>
///     Represents a transfer service deposit response.
/// </summary>
public sealed class DepositResponse : Response
{
    /// <summary>
    ///     (Deprecated, use instructions instead) Terse but complete instructions
    ///     for how to deposit the asset. In the case of most cryptocurrencies it is
    ///     just an address to which the deposit should be sent.
    /// </summary>
    [JsonPropertyName("how")]
    public string? How { get; init; }

    /// <summary>
    ///     The anchor's ID for this deposit. The wallet will use this ID
    ///     to query the /transaction endpoint to check status of the request.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    ///     Estimate of how long the deposit will take to credit in seconds.
    /// </summary>
    [JsonPropertyName("eta")]
    public int? Eta { get; init; }

    /// <summary>
    ///     Minimum amount of an asset that a user can deposit.
    /// </summary>
    [JsonPropertyName("min_amount")]
    public decimal? MinAmount { get; init; }

    /// <summary>
    ///     Maximum amount of asset that a user can deposit.
    /// </summary>
    [JsonPropertyName("max_amount")]
    public decimal? MaxAmount { get; init; }

    /// <summary>
    ///     Fixed fee (if any). In units of the deposited asset.
    /// </summary>
    [JsonPropertyName("fee_fixed")]
    public decimal? FeeFixed { get; init; }

    /// <summary>
    ///     Percentage fee (if any). In units of percentage points.
    /// </summary>
    [JsonPropertyName("fee_percent")]
    public decimal? FeePercent { get; init; }

    /// <summary>
    ///     Object with additional information about the deposit process.
    /// </summary>
    [JsonPropertyName("extra_info")]
    public ExtraInfo? ExtraInfo { get; init; }

    /// <summary>
    ///     A Map containing details that describe how to complete
    ///     the off-chain deposit. The map has SEP-9 financial account fields as keys
    ///     and its values are DepositInstruction objects.
    /// </summary>
    [JsonPropertyName("instructions")]
    public Dictionary<string, DepositInstruction>? Instructions { get; init; }
}

