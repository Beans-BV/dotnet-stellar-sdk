using System;

namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>
///     Thrown when a SEP-7 URI is structurally invalid: wrong scheme, unsupported operation, a missing or
///     malformed parameter, a parameter that does not belong to the operation, or an invalid <c>replace</c>
///     or <c>chain</c> value. <see cref="Exception.Message" /> carries the reason.
/// </summary>
public class InvalidSep7UriException : Sep7Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidSep7UriException" /> class.</summary>
    /// <param name="message">The reason the URI is invalid.</param>
    public InvalidSep7UriException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="InvalidSep7UriException" /> class.</summary>
    /// <param name="message">The reason the URI is invalid.</param>
    /// <param name="innerException">The underlying cause of the failure.</param>
    public InvalidSep7UriException(string message, Exception innerException) : base(message, innerException) { }
}
