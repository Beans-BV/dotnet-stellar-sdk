using System;
using System.Collections.Generic;
using System.Net;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Configuration options for HTTP retry behavior.
/// </summary>
public sealed class HttpRetryOptions
{
    private int _maxRetryCount = 3;
    private int _baseDelayMs = 200;
    private int _maxDelayMs = 5000;

    /// <summary>
    ///     Gets or sets the maximum number of retry attempts. Default is 3.
    ///     Set to 0 to disable retries.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative.</exception>
    public int MaxRetryCount
    {
        get => _maxRetryCount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxRetryCount cannot be negative.");
            }

            _maxRetryCount = value;
        }
    }

    /// <summary>
    ///     Gets or sets the base delay in milliseconds for exponential backoff. Default is 200ms.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public int BaseDelayMs
    {
        get => _baseDelayMs;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "BaseDelayMs must be positive.");
            }

            _baseDelayMs = value;
        }
    }

    /// <summary>
    ///     Gets or sets the maximum delay in milliseconds for exponential backoff. Default is 5000ms.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is not positive.</exception>
    public int MaxDelayMs
    {
        get => _maxDelayMs;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxDelayMs must be positive.");
            }

            _maxDelayMs = value;
        }
    }

    /// <summary>
    ///     Gets or sets whether to use jitter in the backoff calculation. Default is true.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether to honor Retry-After headers. Default is true.
    /// </summary>
    public bool HonorRetryAfterHeader { get; set; } = true;

    /// <summary>
    ///     Gets additional HTTP status codes to consider as retriable.
    ///     By default, the following are retriable: 408, 425, 429, 500, 502, 503, 504.
    /// </summary>
    public ISet<HttpStatusCode> AdditionalRetriableStatusCodes { get; } = new HashSet<HttpStatusCode>();

    /// <summary>
    ///     Gets additional exception types to consider as retriable.
    ///     By default, HttpRequestException, TimeoutException, and TaskCanceledException (from timeouts) are retriable.
    /// </summary>
    public ISet<Type> AdditionalRetriableExceptionTypes { get; } = new HashSet<Type>();
}
