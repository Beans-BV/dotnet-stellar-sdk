using System;

namespace StellarDotnetSdk.Sep.Sep0012.Exceptions;

/// <summary>
///     Thrown when the KYC server responds with <c>413 Payload Too Large</c>, which SEP-0012 prescribes when an
///     uploaded file exceeds the server's size limit (for <c>POST /customer/files</c> or a binary field of
///     <c>PUT /customer</c>).
/// </summary>
public class PayloadTooLargeException : KycServiceException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PayloadTooLargeException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorMessage">The value of the <c>error</c> key in the response body, if any.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    public PayloadTooLargeException(string message, string? errorMessage = null, Exception? innerException = null)
        : base(message, 413, errorMessage, innerException)
    {
    }
}
