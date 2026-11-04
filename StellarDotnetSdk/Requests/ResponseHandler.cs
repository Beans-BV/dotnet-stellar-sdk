using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Deserializes Horizon HTTP responses into strongly-typed objects and translates
///     HTTP error status codes into appropriate SDK exceptions.
/// </summary>
/// <typeparam name="T">The response type to deserialize into.</typeparam>
public class ResponseHandler<T> where T : class
{
    /// <summary>
    ///     Processes an HTTP response, throwing appropriate exceptions for error status codes
    ///     and deserializing the response body for successful responses.
    /// </summary>
    /// <param name="response">The HTTP response message to process.</param>
    /// <returns>The deserialized response object of type <typeparamref name="T" />.</returns>
    /// <exception cref="ClientProtocolException">
    ///     Thrown when the response body is empty, or holds the JSON literal <c>null</c> and so
    ///     deserializes to no object.
    /// </exception>
    public Task<T> HandleResponse(HttpResponseMessage response)
    {
        return HandleResponse(response, null, null);
    }

    /// <summary>
    ///     Processes an HTTP response like <see cref="HandleResponse(HttpResponseMessage)" />, and records on the
    ///     deserialized <see cref="Response" /> the client that sent the request and the URI it was sent to.
    /// </summary>
    /// <param name="response">The HTTP response message to process.</param>
    /// <param name="httpClient">The client that sent the request, or <c>null</c> when unknown.</param>
    /// <param name="requestUri">The URI the request was sent to, or <c>null</c> when unknown.</param>
    /// <returns>The deserialized response object of type <typeparamref name="T" />.</returns>
    /// <exception cref="ClientProtocolException">
    ///     Thrown when the response body is empty, or holds the JSON literal <c>null</c> and so
    ///     deserializes to no object.
    /// </exception>
    internal async Task<T> HandleResponse(HttpResponseMessage response, HttpClient? httpClient, Uri? requestUri)
    {
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        switch ((int)statusCode)
        {
            case (int)HttpStatusCode.ServiceUnavailable:
                throw new ServiceUnavailableException(
                    response.Headers.Contains("Retry-After")
                        ? response.Headers.GetValues("Retry-After").First()
                        : null);
            case (int)HttpStatusCode.TooManyRequests:
                throw new TooManyRequestsException(
                    response.Headers.Contains("Retry-After")
                        ? response.Headers.GetValues("Retry-After").First()
                        : null);
            case >= 300:
                throw new HttpResponseException((int)statusCode, response.ReasonPhrase);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ClientProtocolException("Response contains no content");
        }

        var responseObj = JsonSerializer.Deserialize<T>(content, JsonOptions.DefaultOptions);
        // A body holding the JSON literal null deserializes to no object at all. This method's return type
        // is non-nullable, so passing it on would hand every caller a null through a signature that says
        // otherwise, reaching them as a NullReferenceException that hides the malformed response.
        if (responseObj is null)
        {
            throw new ClientProtocolException("Response contains no object");
        }
        if (responseObj is Response responseInstance)
        {
            responseInstance.SetHeaders(response.Headers);
            responseInstance.HttpClient = httpClient;
            responseInstance.RequestUri = ResolveRequestUri(httpClient, requestUri);
        }

        return responseObj;
    }

    /// <summary>
    ///     Returns the absolute URI the request went to. <see cref="HttpClient" /> resolves a relative request URI
    ///     against its <see cref="HttpClient.BaseAddress" /> before sending, and <see cref="Page{T}" /> follows its
    ///     links on the recorded URI's origin, so the recorded URI has to be the resolved one.
    /// </summary>
    /// <returns>
    ///     <paramref name="requestUri" /> when it is absolute, the resolved URI when it is relative and the client
    ///     has a base address, or <c>null</c> when it cannot be resolved (the request could not have been sent).
    /// </returns>
    private static Uri? ResolveRequestUri(HttpClient? httpClient, Uri? requestUri)
    {
        if (requestUri is null || requestUri.IsAbsoluteUri)
        {
            return requestUri;
        }

        return httpClient?.BaseAddress is { } baseAddress ? new Uri(baseAddress, requestUri) : null;
    }
}