namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the token endpoint returns HTTP 400 with an error message.
/// </summary>
public class SubmitChallengeErrorResponseException : WebAuthException
{
    /// <summary>
    ///     The server's error message describing why the challenge was rejected.
    /// </summary>
    public string Error { get; }

    public SubmitChallengeErrorResponseException(string error)
        : base($"Error requesting jwtToken - error: {error}")
    {
        Error = error;
    }
}

