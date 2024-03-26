using System;
using System.Net.Http;
using System.Threading.Tasks;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk.requests;

public interface IRequestBuilderExecutePageable<T, TResponse> : IRequestBuilder<T>
    where T : class where TResponse : class
{
    /// <Summary>
    ///     Build and execute request.
    /// </Summary>
    Task<Page<TResponse>> Execute();
}

public class RequestBuilderExecutePageable<T, TResponse> : RequestBuilder<T>,
    IRequestBuilderExecutePageable<T, TResponse> where T : class where TResponse : class
{
    protected RequestBuilderExecutePageable(Uri serverUri, string defaultSegment, HttpClient httpClient)
        : base(serverUri, defaultSegment, httpClient)
    {
    }

    /// <inheritdoc />
    public async Task<Page<TResponse>> Execute()
    {
        return await Execute<Page<TResponse>>(BuildUri());
    }
}