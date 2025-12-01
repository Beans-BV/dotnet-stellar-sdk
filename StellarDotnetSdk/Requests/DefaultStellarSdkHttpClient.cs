using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace StellarDotnetSdk.Requests;

public class DefaultStellarSdkHttpClient : HttpClient
{
    /// <summary>
    ///     Creates an HTTP client with some default request headers, retry mechanism, and the given bearer token.
    ///     <para>
    ///         The client includes automatic retry logic for transient failures:
    ///         <list type="bullet">
    ///             <item>Retries up to 3 times by default for transient HTTP errors (408, 425, 429, 500, 502, 503, 504)</item>
    ///             <item>Retries on network exceptions (HttpRequestException, TimeoutException, TaskCanceledException)</item>
    ///             <item>Uses exponential backoff with jitter (default: 200ms base delay, max 5000ms)</item>
    ///             <item>Honors Retry-After headers when present</item>
    ///         </list>
    ///         To customize resilience behavior, pass a custom <see cref="HttpResilienceOptions" /> instance.
    ///         To disable retries, pass <c>new HttpResilienceOptions { MaxRetryCount = 0 }</c>.
    ///     </para>
    /// </summary>
    /// <param name="bearerToken">Bearer token in case the server requires it.</param>
    /// <param name="clientName">Name of the client.</param>
    /// <param name="clientVersion">Version of the client.</param>
    /// <param name="resilienceOptions">Resilience options. If null, default retry configuration is used.</param>
    /// <param name="innerHandler">
    ///     Optional inner HTTP message handler. If null, defaults to <see cref="SocketsHttpHandler" />.
    ///     Use this to inject a custom handler for testing, proxies, or custom certificate handling.
    /// </param>
    public DefaultStellarSdkHttpClient(
        string? bearerToken = null,
        string? clientName = null,
        string? clientVersion = null,
        HttpResilienceOptions? resilienceOptions = null,
        HttpMessageHandler? innerHandler = null)
        : base(CreateHandlerPipeline(resilienceOptions, innerHandler))
    {
        InitializeHeaders(bearerToken, clientName, clientVersion);
    }

    private void InitializeHeaders(string? bearerToken, string? clientName, string? clientVersion)
    {
        var assembly = Assembly.GetAssembly(GetType())!.GetName();
        DefaultRequestHeaders.Add("X-Client-Name", clientName ?? "stellar-dotnet-sdk");
        DefaultRequestHeaders.Add("X-Client-Version", clientVersion ?? assembly.Version!.ToString());
        if (!string.IsNullOrEmpty(bearerToken))
        {
            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }

    private static HttpMessageHandler CreateHandlerPipeline(
        HttpResilienceOptions? resilienceOptions,
        HttpMessageHandler? innerHandler)
    {
        var handler = innerHandler ?? new SocketsHttpHandler();
        return new RetryingHttpMessageHandler(handler, resilienceOptions);
    }
}