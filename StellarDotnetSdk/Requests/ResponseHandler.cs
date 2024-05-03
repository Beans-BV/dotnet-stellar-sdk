using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

public class ResponseHandler<T> where T : class
{
    public async Task<T> HandleResponse(HttpResponseMessage response)
    {
        var statusCode = response.StatusCode;
        var content = await response.Content.ReadAsStringAsync();

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
            throw new ClientProtocolException("Response contains no content");

        var responseObj = JsonSingleton.GetInstance<T>(content);

        if (responseObj is Response responseInstance) responseInstance.SetHeaders(response.Headers);

        return responseObj;
    }
}