using System.Text.Json.Serialization;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a price ratio in Horizon API responses.
/// </summary>
/// <remarks>
///     <para>
///         This class is specifically designed for deserializing price data from Horizon API responses.
///         It uses <see cref="long" /> for numerator and denominator to accommodate values that may exceed
///         the <see cref="int" /> range returned by some Horizon endpoints (e.g., trade aggregations).
///     </para>
///     <para>
///         The Horizon API has inconsistencies in how it returns price data - sometimes as JSON numbers,
///         sometimes as strings. The SDK's <see cref="Converters.JsonOptions.DefaultOptions" /> handles
///         both formats automatically via <c>JsonNumberHandling.AllowReadingFromString</c>.
///     </para>
///     <para>
///         <strong>Important:</strong> This class is for response deserialization only. For building
///         transactions and operations, use <see cref="StellarDotnetSdk.Price" /> instead, which uses
///         <see cref="int" /> values as required by the Stellar XDR protocol.
///     </para>
/// </remarks>
/// <seealso cref="StellarDotnetSdk.Price" />
public class Price
{
    /// <summary>
    ///     The numerator of the price ratio.
    /// </summary>
    [JsonPropertyName("n")]
    public long Numerator { get; init; }

    /// <summary>
    ///     The denominator of the price ratio.
    /// </summary>
    [JsonPropertyName("d")]
    public long Denominator { get; init; }
}