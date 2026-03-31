using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Base exception for all SEP-0010 Web Authentication errors.
/// </summary>
public class WebAuthException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WebAuthException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the SEP-0010 authentication failure.</param>
    public WebAuthException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebAuthException" /> class with a specified error message
    ///     and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">The error message describing the SEP-0010 authentication failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public WebAuthException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}