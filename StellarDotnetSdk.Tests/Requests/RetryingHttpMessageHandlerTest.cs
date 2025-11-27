using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class RetryingHttpMessageHandlerTest
{
    #region Success Cases

    /// <summary>
    /// Verifies that a successful HTTP request (200 OK) does not trigger any retry attempts.
    /// The handler should pass through the response immediately without retrying.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_SuccessfulRequest_NoRetry()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK);

        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        VerifyCallCount(mockHandler, Times.Once());
    }

    /// <summary>
    /// Verifies that when an <see cref="HttpRequestException"/> is thrown on the first attempt,
    /// the handler retries and succeeds on the subsequent attempt.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetriableException_SucceedsOnRetry()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new HttpRequestException("Network error");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    #endregion

    #region Retry Exhaustion Cases

    /// <summary>
    /// Verifies that when a retriable status code is returned on every attempt,
    /// the handler retries up to <see cref="HttpRetryOptions.MaxRetryCount"/> times
    /// and then returns the final failed response.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetriableStatusCode_RetriesUpToMax()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual(3, callCount); // initial + 2 retries
    }

    /// <summary>
    /// Verifies that when a retriable exception is thrown on every attempt,
    /// the handler retries up to the maximum count and then rethrows the exception.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetriableException_ThrowsAfterExhaustingRetries()
    {
        // Arrange
        var mockHandler = CreateMockHandler(() => throw new HttpRequestException("Network error"));

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(
            () => httpClient.SendAsync(request));
        Assert.AreEqual("Network error", ex.Message);
    }

    #endregion

    #region Non-Retriable Cases

    /// <summary>
    /// Verifies that non-retriable status codes (e.g., 400 Bad Request) do not trigger retries.
    /// The response should be returned immediately without any retry attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_NonRetriableStatusCode_NoRetry()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(1, callCount);
    }

    /// <summary>
    /// Verifies that non-retriable exceptions (e.g., <see cref="InvalidOperationException"/>)
    /// are thrown immediately without any retry attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_NonRetriableException_ThrowsImmediately()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            throw new InvalidOperationException("Not retriable");
        });

        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => httpClient.SendAsync(request));
        Assert.AreEqual(1, callCount);
    }

    #endregion

    #region Disabled Retries (MaxRetryCount = 0)

    /// <summary>
    /// Verifies that when <see cref="HttpRetryOptions.MaxRetryCount"/> is set to 0,
    /// no retries are performed even for retriable status codes.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_MaxRetryCountZero_NoRetries()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 0 };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual(1, callCount); // No retries
    }

    #endregion

    #region All Retriable Status Codes

    /// <summary>
    /// Verifies that all retriable HTTP status codes (408, 425, 429, 500, 502, 503, 504)
    /// trigger retry attempts and succeed on the second attempt.
    /// </summary>
    [DataTestMethod]
    [DataRow(HttpStatusCode.RequestTimeout)] // 408
    [DataRow((HttpStatusCode)425)] // TooEarly
    [DataRow((HttpStatusCode)429)] // TooManyRequests
    [DataRow(HttpStatusCode.InternalServerError)] // 500
    [DataRow(HttpStatusCode.BadGateway)] // 502
    [DataRow(HttpStatusCode.ServiceUnavailable)] // 503
    [DataRow(HttpStatusCode.GatewayTimeout)] // 504
    public async Task SendAsync_AllRetriableStatusCodes_Retries(HttpStatusCode statusCode)
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return callCount == 1
                ? new HttpResponseMessage(statusCode)
                : new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 1, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    #endregion

    #region Retriable Exceptions

    /// <summary>
    /// Verifies that <see cref="TimeoutException"/> is treated as retriable
    /// and triggers a retry attempt that succeeds on the second call.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_TimeoutException_Retries()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new TimeoutException("Timed out");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    /// <summary>
    /// Verifies that <see cref="TaskCanceledException"/> thrown due to timeout
    /// (as opposed to user cancellation) is treated as retriable and triggers a retry.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_TaskCanceledExceptionFromTimeout_Retries()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                // TaskCanceledException with default token (simulates HttpClient.Timeout)
                throw new TaskCanceledException("The request was canceled due to timeout.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    #endregion

    #region Cancellation

    /// <summary>
    /// Verifies that when a user-provided <see cref="CancellationToken"/> is canceled,
    /// the handler stops retrying and propagates the cancellation exception.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_UserCancellation_NoRetry()
    {
        // Arrange
        var callCount = 0;
        var cts = new CancellationTokenSource();
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 3, BaseDelayMs = 500 };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Cancel after a short delay (during retry wait)
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            cts.Cancel();
        });

        // Act & Assert
        try
        {
            await httpClient.SendAsync(request, cts.Token);
            Assert.Fail("Expected cancellation exception");
        }
        catch (OperationCanceledException)
        {
            // Expected - TaskCanceledException inherits from OperationCanceledException
        }

        Assert.IsTrue(callCount >= 1, $"Expected at least 1 call, got {callCount}");
        Assert.IsTrue(callCount <= 2, $"Expected at most 2 calls, got {callCount}");
    }

    /// <summary>
    /// Verifies that cancellation during an active request propagates immediately
    /// without waiting for retry delays.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_CancellationDuringRequest_PropagatesImmediately()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (_, ct) =>
            {
                cts.Cancel();
                ct.ThrowIfCancellationRequested();
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act & Assert
        try
        {
            await httpClient.SendAsync(request, cts.Token);
            Assert.Fail("Expected cancellation exception");
        }
        catch (OperationCanceledException)
        {
            // Expected - TaskCanceledException inherits from OperationCanceledException
        }
    }

    #endregion

    #region Retry-After Header

    /// <summary>
    /// Verifies that when a response includes a Retry-After header with a numeric value (seconds),
    /// the handler honors the delay specified in the header before retrying.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeaderInSeconds_Honored()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Retry-After", "1"); // 1 second
                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 3,
            HonorRetryAfterHeader = true
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var startTime = DateTime.UtcNow;

        // Act
        var response = await httpClient.SendAsync(request);

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
        Assert.IsTrue(elapsed.TotalMilliseconds >= 900, $"Expected at least 900ms delay, got {elapsed.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Verifies that when a response includes a Retry-After header with an RFC 1123 datetime value,
    /// the handler calculates and honors the delay until that datetime before retrying.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeaderAsDateTime_Honored()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    // Use a fixed 500ms delay - enough to be distinguishable from exponential backoff
                    var retryDate = DateTimeOffset.UtcNow.AddMilliseconds(500);
                    response.Headers.Add("Retry-After", retryDate.ToString("R")); // RFC 1123 format
                    return response;
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 3,
            BaseDelayMs = 10, // Very short base delay to distinguish from Retry-After
            HonorRetryAfterHeader = true,
            UseJitter = false
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var startTime = DateTimeOffset.UtcNow;

        // Act
        var response = await httpClient.SendAsync(request);

        var elapsed = DateTimeOffset.UtcNow - startTime;

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
        // Should have waited significantly longer than the 10ms base delay
        // The Retry-After header should cause ~500ms delay (with some variance due to RFC 1123 second-level precision)
        Assert.IsTrue(elapsed.TotalMilliseconds >= 100, $"Expected delay from Retry-After header, got {elapsed.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Verifies that when <see cref="HttpRetryOptions.HonorRetryAfterHeader"/> is disabled,
    /// the handler ignores Retry-After headers and uses exponential backoff instead.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeaderDisabled_UsesExponentialBackoff()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Retry-After", "10"); // 10 seconds - would be very slow if honored
                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 3,
            BaseDelayMs = 10,
            HonorRetryAfterHeader = false, // Disabled
            UseJitter = false
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var startTime = DateTime.UtcNow;

        // Act
        var response = await httpClient.SendAsync(request);

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        // Should complete quickly since Retry-After is ignored
        Assert.IsTrue(elapsed.TotalMilliseconds < 1000, $"Expected less than 1000ms, got {elapsed.TotalMilliseconds}ms");
    }

    /// <summary>
    /// Verifies that when a Retry-After header contains a datetime in the past,
    /// the handler treats it as zero delay and retries immediately.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RetryAfterHeader_PastDateTime_TreatedAsZero()
    {
        // Arrange
        var callCount = 0;
        var pastDate = DateTime.UtcNow.AddSeconds(-10); // 10 seconds in the past
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Retry-After", pastDate.ToString("R")); // RFC 1123 format, past date
                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 3,
            HonorRetryAfterHeader = true
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var startTime = DateTime.UtcNow;

        // Act - should not throw even with past Retry-After date
        var response = await httpClient.SendAsync(request);

        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
        // Should complete quickly (past date results in negative delay which is treated as 0)
        Assert.IsTrue(elapsed.TotalMilliseconds < 1000, $"Expected quick response, got {elapsed.TotalMilliseconds}ms");
    }

    #endregion

    #region Request Content Preservation

    /// <summary>
    /// Verifies that when a request with content (e.g., POST body) fails and is retried,
    /// the request content is preserved and sent again on the retry attempt.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_RequestWithContent_ContentPreservedOnRetry()
    {
        // Arrange
        var capturedContent = "";
        var callCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (req, _) =>
            {
                callCount++;
                capturedContent = req.Content != null ? await req.Content.ReadAsStringAsync() : "";

                if (callCount == 1)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com")
        {
            Content = new StringContent("test content")
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
        Assert.AreEqual("test content", capturedContent);
    }

    #endregion

    #region Custom Retriable Configuration

    /// <summary>
    /// Verifies that status codes added to <see cref="HttpRetryOptions.AdditionalRetriableStatusCodes"/>
    /// are treated as retriable and trigger retry attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_AdditionalRetriableStatusCode_Retries()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            return callCount == 1
                ? new HttpResponseMessage(HttpStatusCode.Conflict) // 409 - not retriable by default
                : new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 2,
            BaseDelayMs = 1,
            UseJitter = false
        };
        retryOptions.AdditionalRetriableStatusCodes.Add(HttpStatusCode.Conflict);
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    /// <summary>
    /// Verifies that exception types added to <see cref="HttpRetryOptions.AdditionalRetriableExceptionTypes"/>
    /// are treated as retriable and trigger retry attempts.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_AdditionalRetriableExceptionType_Retries()
    {
        // Arrange
        var callCount = 0;
        var mockHandler = CreateMockHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("Retriable custom exception");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 2,
            BaseDelayMs = 1,
            UseJitter = false
        };
        retryOptions.AdditionalRetriableExceptionTypes.Add(typeof(InvalidOperationException));
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount);
    }

    #endregion

    #region Options Validation

    /// <summary>
    /// Verifies that setting <see cref="HttpRetryOptions.MaxRetryCount"/> to a negative value
    /// throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [TestMethod]
    public void HttpRetryOptions_NegativeMaxRetryCount_Throws()
    {
        var options = new HttpRetryOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxRetryCount = -1);
    }

    /// <summary>
    /// Verifies that setting <see cref="HttpRetryOptions.BaseDelayMs"/> to zero
    /// throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [TestMethod]
    public void HttpRetryOptions_ZeroBaseDelayMs_Throws()
    {
        var options = new HttpRetryOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.BaseDelayMs = 0);
    }

    /// <summary>
    /// Verifies that setting <see cref="HttpRetryOptions.MaxDelayMs"/> to a negative value
    /// throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [TestMethod]
    public void HttpRetryOptions_NegativeMaxDelayMs_Throws()
    {
        var options = new HttpRetryOptions();
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => options.MaxDelayMs = -1);
    }

    #endregion

    #region Constructor Validation

    /// <summary>
    /// Verifies that passing a null inner handler to the constructor
    /// throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullInnerHandler_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new RetryingHttpMessageHandler(null!));
    }

    #endregion

    #region Headers Preservation

    /// <summary>
    /// Verifies that when a request with custom headers fails and is retried,
    /// all request headers are preserved and sent again on the retry attempt.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_CustomHeaders_PreservedOnRetry()
    {
        // Arrange
        // Use case-insensitive dictionary since HTTP headers are case-insensitive
        var capturedHeaders = new List<Dictionary<string, string>>();
        var callCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                callCount++;
                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var header in req.Headers)
                {
                    headers[header.Key] = string.Join(",", header.Value);
                }

                capturedHeaders.Add(headers);
            })
            .ReturnsAsync(() => callCount == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : new HttpResponseMessage(HttpStatusCode.OK));

        var retryOptions = new HttpRetryOptions { MaxRetryCount = 2, BaseDelayMs = 1, UseJitter = false };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Add("X-Custom-Header", "custom-value");
        request.Headers.Add("X-Request-Id", "12345");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, callCount, "Should have 2 calls (initial + 1 retry)");
        Assert.AreEqual(2, capturedHeaders.Count, "Should have captured headers from 2 calls");

        // Verify headers on the retry attempt (second call)
        var retryHeaders = capturedHeaders[1];
        Assert.IsTrue(retryHeaders.ContainsKey("X-Custom-Header"), "X-Custom-Header should be present on retry");
        Assert.AreEqual("custom-value", retryHeaders["X-Custom-Header"]);
        Assert.IsTrue(retryHeaders.ContainsKey("X-Request-Id"), "X-Request-Id should be present on retry");
        Assert.AreEqual("12345", retryHeaders["X-Request-Id"]);
    }

    #endregion

    #region Exponential Backoff Timing

    /// <summary>
    /// Verifies that retry delays follow an exponential backoff pattern,
    /// with each retry waiting approximately twice as long as the previous delay.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_ExponentialBackoff_DelaysIncrease()
    {
        // Arrange
        var callTimes = new List<DateTime>();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callTimes.Add(DateTime.UtcNow);
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 3,
            BaseDelayMs = 50,
            UseJitter = false
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(4, callTimes.Count); // initial + 3 retries

        // Check delays: 50ms, 100ms, 200ms (exponential)
        var delay1 = (callTimes[1] - callTimes[0]).TotalMilliseconds;
        var delay2 = (callTimes[2] - callTimes[1]).TotalMilliseconds;
        var delay3 = (callTimes[3] - callTimes[2]).TotalMilliseconds;

        // Allow 20% margin for timing variations
        Assert.IsTrue(delay1 >= 40 && delay1 <= 70, $"Delay 1 expected ~50ms, got {delay1}ms");
        Assert.IsTrue(delay2 >= 80 && delay2 <= 130, $"Delay 2 expected ~100ms, got {delay2}ms");
        Assert.IsTrue(delay3 >= 160 && delay3 <= 260, $"Delay 3 expected ~200ms, got {delay3}ms");
    }

    /// <summary>
    /// Verifies that when <see cref="HttpRetryOptions.MaxDelayMs"/> is set,
    /// retry delays are capped at the maximum value and do not exceed it.
    /// </summary>
    [TestMethod]
    public async Task SendAsync_MaxDelayMs_CapsDelay()
    {
        // Arrange
        var callTimes = new List<DateTime>();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callTimes.Add(DateTime.UtcNow);
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            });

        var retryOptions = new HttpRetryOptions
        {
            MaxRetryCount = 2,
            BaseDelayMs = 100,
            MaxDelayMs = 150, // Cap at 150ms
            UseJitter = false
        };
        var retryHandler = new RetryingHttpMessageHandler(mockHandler.Object, retryOptions);
        using var httpClient = new HttpClient(retryHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.AreEqual(3, callTimes.Count);

        // Second retry would be 200ms but capped at 150ms
        var delay2 = (callTimes[2] - callTimes[1]).TotalMilliseconds;
        Assert.IsTrue(delay2 <= 180, $"Delay 2 should be capped at ~150ms, got {delay2}ms");
    }

    #endregion

    #region Helper Methods

    private static Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));
        return mockHandler;
    }

    private static Mock<HttpMessageHandler> CreateMockHandler(Func<HttpResponseMessage> responseFactory)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseFactory);
        return mockHandler;
    }

    private static void VerifyCallCount(Mock<HttpMessageHandler> mockHandler, Times times)
    {
        mockHandler.Protected().Verify(
            "SendAsync",
            times,
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}
