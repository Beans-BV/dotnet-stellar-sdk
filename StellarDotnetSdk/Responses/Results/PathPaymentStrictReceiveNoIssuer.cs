using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses.Results;

/// <summary>
///     Missing issuer on one asset.
/// </summary>
public class PathPaymentStrictReceiveNoIssuer : PathPaymentStrictReceiveResult
{
    /// <summary>
    ///     The asset that caused the error.
    /// </summary>
    public Asset NoIssuer { get; set; }
}