using System;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Exception thrown when the anchor returns an error response.
///     This exception is thrown when the server responds with an error object containing an error message.
///     The error field contains the anchor's error description.
/// </summary>
public class Sep24RequestException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24RequestException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message provided by the anchor.</param>
    public Sep24RequestException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24RequestException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public Sep24RequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

