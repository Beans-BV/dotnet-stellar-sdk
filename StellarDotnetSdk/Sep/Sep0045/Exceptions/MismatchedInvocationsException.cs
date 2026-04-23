namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when the invocations across SEP-45 challenge authorization entries do not match each other.</summary>
public class MismatchedInvocationsException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="MismatchedInvocationsException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public MismatchedInvocationsException(string message) : base(message) { }
}
