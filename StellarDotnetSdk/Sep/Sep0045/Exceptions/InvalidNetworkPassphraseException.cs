namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>
///     Thrown when the SEP-45 challenge response's <c>network_passphrase</c> does not match the
///     network the client is configured for.
/// </summary>
public class InvalidNetworkPassphraseException : InvalidSep45ChallengeException
{
    /// <summary>Initializes a new instance of the <see cref="InvalidNetworkPassphraseException" /> class.</summary>
    /// <param name="message">The validation error message.</param>
    public InvalidNetworkPassphraseException(string message) : base(message)
    {
    }
}