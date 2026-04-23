namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Thrown when a SEP-45 challenge authorization entry contains disallowed sub-invocations.</summary>
public class SubInvocationsNotAllowedException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="SubInvocationsNotAllowedException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public SubInvocationsNotAllowedException(string message) : base(message) { }
}
