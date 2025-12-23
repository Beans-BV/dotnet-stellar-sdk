namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the token endpoint returns an unexpected HTTP status code.
/// </summary>
public class SubmitChallengeUnknownResponseException : WebAuthException
{
    /// <summary>
    ///     The HTTP status code received.
    /// </summary>
    public int Code { get; }

    /// <summary>
    ///     The HTTP response body.
    /// </summary>
    public string Body { get; }

    public SubmitChallengeUnknownResponseException(int code, string body)
        : base($"Unknown response - code: {code} - body: {body}")
    {
        Code = code;
        Body = body;
    }
}

