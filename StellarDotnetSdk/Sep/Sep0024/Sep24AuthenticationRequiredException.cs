using System;

namespace StellarDotnetSdk.Sep.Sep0024;

/// <summary>
///     Exception thrown when SEP-24 endpoint requires authentication but no valid JWT token was provided.
///     This exception is thrown when an endpoint requires SEP-10 authentication (JWT token) but the request
///     was made without authentication or with invalid credentials.
/// </summary>
public class Sep24AuthenticationRequiredException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24AuthenticationRequiredException" /> class.
    /// </summary>
    public Sep24AuthenticationRequiredException()
        : base("The endpoint requires authentication.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24AuthenticationRequiredException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public Sep24AuthenticationRequiredException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sep24AuthenticationRequiredException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public Sep24AuthenticationRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

