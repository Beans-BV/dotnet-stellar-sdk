using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     HTTP message handler that implements retry logic with exponential backoff for transient failures.
/// </summary>
public class RetryingHttpMessageHandler : DelegatingHandler
{
    private static readonly HttpStatusCode[] DefaultRetriableStatusCodes =
    {
        HttpStatusCode.RequestTimeout, // 408
        (HttpStatusCode)425, // TooEarly - not available in all .NET versions
        HttpStatusCode.TooManyRequests, // 429
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway, // 502
        HttpStatusCode.ServiceUnavailable, // 503
        HttpStatusCode.GatewayTimeout // 504
    };

    private readonly HttpRetryOptions _options;
    private readonly Random _random;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryingHttpMessageHandler" /> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler to delegate to.</param>
    /// <param name="options">The retry options. If null, default options are used.</param>
    /// <exception cref="ArgumentNullException">Thrown when innerHandler is null.</exception>
    public RetryingHttpMessageHandler(HttpMessageHandler innerHandler, HttpRetryOptions? options = null)
        : this(innerHandler, options, new Random())
    {
    }

    /// <summary>
    ///     Initializes a new instance for testing with injectable random.
    /// </summary>
    internal RetryingHttpMessageHandler(HttpMessageHandler innerHandler, HttpRetryOptions? options, Random random)
        : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
    {
        _options = options ?? new HttpRetryOptions();
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Buffer the original request content so we can clone it for retries
        byte[]? contentBytes = null;
        if (request.Content != null)
        {
            contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        var attempt = 0;
        Exception? lastException = null;
        HttpResponseMessage? lastResponse = null;

        while (attempt <= _options.MaxRetryCount)
        {
            HttpRequestMessage? requestToSend = null;
            try
            {
                // Clone the request for this attempt
                requestToSend = CloneRequest(request, contentBytes);

                var response = await base.SendAsync(requestToSend, cancellationToken).ConfigureAwait(false);

                // If successful or non-retriable status code, return immediately
                if (response.IsSuccessStatusCode || !IsRetriableStatusCode(response.StatusCode))
                {
                    lastResponse?.Dispose();
                    return response;
                }

                // Store the response for potential return
                lastResponse?.Dispose();
                lastResponse = response;

                // If we've exhausted retries, return the last response
                if (attempt >= _options.MaxRetryCount)
                {
                    return lastResponse;
                }

                // Calculate delay and wait
                var delay = CalculateDelay(attempt, response);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // User cancellation - don't retry, propagate immediately
                lastResponse?.Dispose();
                throw;
            }
            catch (Exception ex) when (IsRetriableException(ex, cancellationToken) && attempt < _options.MaxRetryCount)
            {
                lastException = ex;
                var delay = CalculateDelay(attempt, null);
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // User cancellation during delay
                    lastResponse?.Dispose();
                    throw;
                }
            }
            finally
            {
                // Always dispose the cloned request after each attempt
                requestToSend?.Dispose();
            }

            attempt++;
        }

        // If we get here, all retries failed with exceptions
        if (lastException != null)
        {
            lastResponse?.Dispose();
            throw lastException;
        }

        // This path means we exhausted retries with retriable status codes
        if (lastResponse != null)
        {
            return lastResponse;
        }

        // This should not happen, but handle it gracefully
        throw new HttpRequestException("Request failed after all retry attempts.");
    }

    private bool IsRetriableStatusCode(HttpStatusCode statusCode)
    {
        if (Array.IndexOf(DefaultRetriableStatusCodes, statusCode) >= 0)
        {
            return true;
        }

        return _options.AdditionalRetriableStatusCodes.Contains(statusCode);
    }

    private bool IsRetriableException(Exception exception, CancellationToken cancellationToken)
    {
        // Check additional retriable exception types first
        var exceptionType = exception.GetType();
        if (_options.AdditionalRetriableExceptionTypes.Any(t => t.IsAssignableFrom(exceptionType)))
        {
            return true;
        }

        // HttpRequestException - network errors, always retriable
        if (exception is HttpRequestException)
        {
            return true;
        }

        // TimeoutException - always retriable
        if (exception is TimeoutException)
        {
            return true;
        }

        // TaskCanceledException - only retriable if it's a timeout, not user cancellation
        if (exception is TaskCanceledException)
        {
            // If cancellation was requested by user, don't retry
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            // Otherwise, it's likely a timeout (HttpClient.Timeout), so retry
            return true;
        }

        return false;
    }

    private int CalculateDelay(int attempt, HttpResponseMessage? response)
    {
        // Honor Retry-After header if present and enabled
        if (_options.HonorRetryAfterHeader && response != null)
        {
            var retryAfterDelay = ParseRetryAfterHeader(response);
            if (retryAfterDelay.HasValue)
            {
                return retryAfterDelay.Value;
            }
        }

        // Exponential backoff: baseDelay * 2^attempt
        // Use Math.Min to prevent overflow before multiplication
        var exponentialDelay = Math.Min(_options.BaseDelayMs * (1 << attempt), _options.MaxDelayMs);
        var delay = (int)exponentialDelay;

        // Apply jitter if enabled (random value between 0.8 and 1.2 of the delay)
        if (_options.UseJitter)
        {
            var jitterFactor = 0.8 + (_random.NextDouble() * 0.4); // 0.8 to 1.2
            delay = (int)(delay * jitterFactor);
        }

        return Math.Min(delay, _options.MaxDelayMs);
    }

    private int? ParseRetryAfterHeader(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var retryAfterValues))
        {
            return null;
        }

        var retryAfterValue = retryAfterValues.FirstOrDefault();
        if (string.IsNullOrEmpty(retryAfterValue))
        {
            return null;
        }

        // Try parsing as integer (seconds)
        if (int.TryParse(retryAfterValue, out var seconds))
        {
            return Math.Max(0, Math.Min(seconds * 1000, _options.MaxDelayMs));
        }

        // Try parsing as DateTime (RFC 7231 / HTTP-date format)
        if (DateTimeOffset.TryParse(retryAfterValue, out var retryDate))
        {
            var retryDelayMs = (int)(retryDate - DateTimeOffset.UtcNow).TotalMilliseconds;
            return Math.Max(0, Math.Min(retryDelayMs, _options.MaxDelayMs));
        }

        return null;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? contentBytes)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
            VersionPolicy = original.VersionPolicy
        };

        // Copy headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (contentBytes != null)
        {
            clone.Content = new ByteArrayContent(contentBytes);

            // Copy content headers from original
            if (original.Content != null)
            {
                foreach (var header in original.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        // Copy options (replaces deprecated Properties in .NET 5+)
        // Use IDictionary interface for compatibility
        var originalOptions = (IDictionary<string, object?>)original.Options;
        var cloneOptions = (IDictionary<string, object?>)clone.Options;
        foreach (var option in originalOptions)
        {
            cloneOptions[option.Key] = option.Value;
        }

        return clone;
    }
}
