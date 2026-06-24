using System;

namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the arguments of the SEP-45 challenge invocation are invalid or unexpected.</summary>
public class InvalidArgumentsException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidArgumentsException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidArgumentsException(string message) : base(message) { }

    /// <summary>Initializes a new instance that wraps the underlying cause.</summary>
    /// <param name="message">The validation error message.</param>
    /// <param name="innerException">The underlying cause (e.g. a malformed-XDR decode exception).</param>
    public InvalidArgumentsException(string message, Exception innerException) : base(message, innerException) { }
}
