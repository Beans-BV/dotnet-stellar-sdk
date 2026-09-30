using System;

namespace StellarDotnetSdk.Sep.Sep0012.Exceptions;

/// <summary>
///     Thrown when the KYC server rejects the request as unauthenticated: a <c>401 Unauthorized</c> response, or a
///     <c>403 Forbidden</c> response whose body has <c>"type": "authentication_required"</c>. Obtain a fresh JWT via
///     SEP-10 (<see cref="StellarDotnetSdk.Sep.Sep0010.ClientWebAuth" />) or SEP-45
///     (<see cref="StellarDotnetSdk.Sep.Sep0045.ClientWebAuthContract" />) and retry.
/// </summary>
public class AuthenticationRequiredException : KycServiceException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthenticationRequiredException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code of the response (401 or 403).</param>
    /// <param name="errorMessage">The value of the <c>error</c> key in the response body, if any.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    public AuthenticationRequiredException(
        string message,
        int statusCode,
        string? errorMessage = null,
        Exception? innerException = null)
        : base(message, statusCode, errorMessage, innerException)
    {
    }
}
