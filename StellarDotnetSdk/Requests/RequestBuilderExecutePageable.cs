using System;
using System.Net.Http;
using System.Threading.Tasks;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Requests;

/// <summary>
///     Extends <see cref="IRequestBuilder{T}" /> with the ability to execute a request and return a paginated response.
/// </summary>
/// <typeparam name="T">The concrete request builder type (for fluent chaining).</typeparam>
/// <typeparam name="TResponse">The response element type contained in each page.</typeparam>
public interface IRequestBuilderExecutePageable<T, TResponse> : IRequestBuilder<T>
    where T : class where TResponse : class
{
    /// <Summary>
    ///     Build and execute request.
    /// </Summary>
    Task<Page<TResponse>> Execute();
}

/// <summary>
///     Base class for request builders that execute requests and return a <see cref="Page{T}" /> of results.
/// </summary>
/// <typeparam name="T">The concrete request builder type (for fluent chaining).</typeparam>
/// <typeparam name="TResponse">The response element type contained in each page.</typeparam>
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