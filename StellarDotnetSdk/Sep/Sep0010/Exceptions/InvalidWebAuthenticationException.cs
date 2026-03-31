using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when WebAuthentication verification fails.
/// </summary>
public class InvalidWebAuthenticationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidWebAuthenticationException" /> class.
    /// </summary>
    /// <param name="message">The error message describing the verification failure.</param>
    public InvalidWebAuthenticationException(string message) : base(message)
    {
    }
}