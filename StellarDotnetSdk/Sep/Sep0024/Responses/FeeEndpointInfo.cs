using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0024.Responses;

/// <summary>
///     Information about the /fee endpoint availability and requirements.
///     Indicates whether the anchor provides a separate fee endpoint for querying fees,
///     and whether authentication is required to access it.
/// </summary>
public class FeeEndpointInfo : Response
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FeeEndpointInfo" /> class.
    /// </summary>
    /// <param name="enabled">True if the /fee endpoint is available.</param>
    /// <param name="authenticationRequired">True if client must be authenticated (SEP-10 JWT) before accessing the fee endpoint.</param>
    [JsonConstructor]
    public FeeEndpointInfo(bool enabled, bool authenticationRequired = false)
    {
        Enabled = enabled;
        AuthenticationRequired = authenticationRequired;
    }

    /// <summary>
    ///     Gets a value indicating whether the /fee endpoint is available.
    ///     If false, all fee information is provided in the deposit/withdraw asset objects.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; }

    /// <summary>
    ///     Gets a value indicating whether client must be authenticated (SEP-10 JWT) before accessing the fee endpoint.
    /// </summary>
    [JsonPropertyName("authentication_required")]
    public bool AuthenticationRequired { get; }
}

