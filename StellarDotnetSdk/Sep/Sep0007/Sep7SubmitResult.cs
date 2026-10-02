using System.Net;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Sep.Sep0007;

/// <summary>
///     The outcome of handing a signed SEP-7 transaction on: either POSTed to the request's <c>callback</c>
///     URL, or submitted to Horizon when the request has no callback.
/// </summary>
public sealed class Sep7SubmitResult
{
    internal Sep7SubmitResult(HttpStatusCode callbackStatusCode, string callbackResponseBody)
    {
        SubmittedToCallback = true;
        CallbackStatusCode = callbackStatusCode;
        CallbackResponseBody = callbackResponseBody;
    }

    internal Sep7SubmitResult(SubmitTransactionResponse? horizonResponse)
    {
        SubmittedToCallback = false;
        HorizonResponse = horizonResponse;
    }

    /// <summary>
    ///     <c>true</c> when the signed transaction was POSTed to the callback URL; <c>false</c> when it was
    ///     submitted to Horizon.
    /// </summary>
    public bool SubmittedToCallback { get; }

    /// <summary>The HTTP status code the callback URL answered with; <c>null</c> for a Horizon submission.</summary>
    public HttpStatusCode? CallbackStatusCode { get; }

    /// <summary>
    ///     The body the callback URL answered with (at most 512 KiB); <c>null</c> for a Horizon submission.
    ///     SEP-7 defines no format for it, so it is returned as-is, decoded by its byte order mark if it starts with
    ///     one (UTF-8 or UTF-16, the mark dropped), else with the charset its <c>Content-Type</c> declares if .NET
    ///     supports it, else as UTF-8. Without <c>CodePagesEncodingProvider</c> registered, .NET supports only the
    ///     UTF encodings, US-ASCII and ISO-8859-1 (true Latin-1, not windows-1252 as browsers read it); undecodable
    ///     bytes become U+FFFD.
    /// </summary>
    public string? CallbackResponseBody { get; }

    /// <summary>Horizon's answer to the submission; <c>null</c> when the transaction went to the callback.</summary>
    public SubmitTransactionResponse? HorizonResponse { get; }

    /// <summary>
    ///     For a callback, whether it answered with a 2xx status: SEP-7 defines nothing about the callback's reply,
    ///     so that is all the SDK can tell (a callback may still report a problem in a 2xx body, in
    ///     <see cref="CallbackResponseBody" />). For Horizon, <see cref="SubmitTransactionResponse.IsSuccess" />;
    ///     <c>false</c> as well when Horizon returned no response. Neither a callback's non-2xx answer nor a
    ///     transaction Horizon rejects is thrown, so check this before telling the user the request is done.
    /// </summary>
    public bool IsSuccess => SubmittedToCallback
        ? CallbackStatusCode is { } status && (int)status >= 200 && (int)status <= 299
        : HorizonResponse?.IsSuccess == true;
}
