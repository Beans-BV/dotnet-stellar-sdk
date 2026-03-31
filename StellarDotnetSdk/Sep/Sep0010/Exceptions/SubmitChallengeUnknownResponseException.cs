namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the token endpoint returns an unexpected HTTP status code.
/// </summary>
public class SubmitChallengeUnknownResponseException : WebAuthException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubmitChallengeUnknownResponseException" /> class.
    /// </summary>
    /// <param name="code">The HTTP status code received.</param>
    /// <param name="body">The HTTP response body.</param>
    public SubmitChallengeUnknownResponseException(int code, string body)
        : base($"Unknown response - code: {code} - body: {body}")
    {
        Code = code;
        Body = body;
    }

    /// <summary>
    ///     The HTTP status code received.
    /// </summary>
    public int Code { get; }

    /// <summary>
    ///     The HTTP response body.
    /// </summary>
    public string Body { get; }
}