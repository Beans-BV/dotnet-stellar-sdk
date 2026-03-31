using System;

namespace StellarDotnetSdk.Sep.Sep0001.Exceptions;

/// <summary>
///     Exception thrown when stellar.toml parsing or fetching fails.
/// </summary>
public class StellarTomlException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StellarTomlException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing the stellar.toml failure.</param>
    public StellarTomlException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StellarTomlException" /> class with a specified error message
    ///     and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">The error message describing the stellar.toml failure.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public StellarTomlException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}