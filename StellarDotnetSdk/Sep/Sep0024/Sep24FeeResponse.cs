using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Response from the /fee endpoint containing the calculated fee.
///     Contains the exact fee that would be charged for a specific deposit or withdrawal operation with the given parameters.
/// </summary>
public class Sep24FeeResponse : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24FeeResponse" /> class.
    /// </summary>
    /// <param name="fee">The total fee (in units of the asset involved) that would be charged to deposit/withdraw the specified amount.</param>
    [JsonConstructor]
    public Sep24FeeResponse(decimal? fee = null)
    {
        Fee = fee;
    }

    /// <summary>
    ///     Gets the total fee (in units of the asset involved) that would be charged
    ///     to deposit/withdraw the specified amount.
    /// </summary>
    [JsonPropertyName("fee")]
    public decimal? Fee { get; }
}

