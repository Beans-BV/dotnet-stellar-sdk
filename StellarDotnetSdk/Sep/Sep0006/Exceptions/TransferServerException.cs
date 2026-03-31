using System;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Base exception for SEP-6 transfer server errors.
///     Thrown when a transfer server operation fails.
/// </summary>
public class TransferServerException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferServerException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the transfer server failure.</param>
    public TransferServerException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransferServerException" /> class with a specified error message
    ///     and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">The error message describing the transfer server failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public TransferServerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}