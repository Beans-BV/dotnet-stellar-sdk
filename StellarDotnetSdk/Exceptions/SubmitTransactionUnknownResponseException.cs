using System;
using System.Net;

namespace stellar_dotnet_sdk;

public class SubmitTransactionUnknownResponseException : Exception
{
    public SubmitTransactionUnknownResponseException(HttpStatusCode code, string body) :
        base($"Unknown response from Horizon - code: {code} - body: {body}")
    {
        Body = body;
        Code = code;
    }

    public HttpStatusCode Code { get; }
    public string Body { get; }
}