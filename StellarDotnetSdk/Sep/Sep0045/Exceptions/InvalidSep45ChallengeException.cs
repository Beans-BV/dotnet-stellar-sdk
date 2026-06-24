using System;

namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Base exception for SEP-45 challenge validation failures.</summary>
public class InvalidSep45ChallengeException : WebAuthContractException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidSep45ChallengeException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidSep45ChallengeException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="InvalidSep45ChallengeException" /> class that wraps an inner exception.</summary>
    /// <param name="message">The validation error message.</param>
    /// <param name="innerException">The exception that caused this validation failure.</param>
    public InvalidSep45ChallengeException(string message, Exception innerException)
        : base(message, innerException) { }
}
