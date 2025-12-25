namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when the challenge request endpoint returns an error.
/// </summary>
public class ChallengeRequestErrorException : WebAuthException
{
    /// <summary>
    ///     The HTTP status code from the error response.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    ///     The response body content.
    /// </summary>
    public string? ResponseBody { get; }

    public ChallengeRequestErrorException(int statusCode, string? responseBody = null)
        : base($"Challenge request failed with status code {statusCode}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

