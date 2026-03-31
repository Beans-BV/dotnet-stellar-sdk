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
    public async Task<T> HandleResponse(HttpResponseMessage response)
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
        if (responseObj is Response responseInstance)
        {
            responseInstance.SetHeaders(response.Headers);
        }

        return responseObj;
    }
}