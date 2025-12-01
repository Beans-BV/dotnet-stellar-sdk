using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace StellarDotnetSdk.Requests;

public class DefaultStellarSdkHttpClient : HttpClient
{
    /// <summary>
    ///     Creates an HTTP client with some default request headers and the given bearer token.
    ///     <para>
    ///         By default, no automatic retries are enabled.
    ///         To enable retries for connection failures (network errors, DNS failures), pass a custom
    ///         <see cref="HttpResilienceOptions" /> instance with <c>MaxRetryCount</c> set to a positive value.
    ///         Note: HTTP error status codes (4xx/5xx) are never retried automatically.
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
        
        // Add resilience handler if any resilience feature is enabled (retries, circuit breaker, or timeout)
        if (resilienceOptions != null && resilienceOptions.HasAnyResilienceFeatureEnabled)
        {
            return new RetryingHttpMessageHandler(handler, resilienceOptions);
        }
        
        return handler;
    }
}