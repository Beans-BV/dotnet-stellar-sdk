using System;

namespace StellarDotnetSdk.Sep.Sep0012.Exceptions;

/// <summary>
///     Thrown when the KYC server responds with <c>404 Not Found</c> to a <c>/customer</c> request: the customer
///     <c>id</c> is unknown, belongs to a different Stellar account, or the anchor holds no information on the
///     customer being updated or deleted.
/// </summary>
public class CustomerNotFoundException : KycServiceException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerNotFoundException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorMessage">The value of the <c>error</c> key in the response body, if any.</param>
    /// <param name="innerException">The exception that caused this one, if any.</param>
    public CustomerNotFoundException(string message, string? errorMessage = null, Exception? innerException = null)
        : base(message, 404, errorMessage, innerException)
    {
    }
}
