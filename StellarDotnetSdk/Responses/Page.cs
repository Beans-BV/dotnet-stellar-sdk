using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StellarDotnetSdk.Responses;
#nullable disable

/// <summary>
///     Represents page of objects.
/// </summary>
public class Page<T> : Response
{
    [JsonProperty(PropertyName = "_embedded")]
    public EmbeddedRecords Embedded { get; init; }

    public List<T> Records => Embedded.Records;

    [JsonProperty(PropertyName = "_links")]
    public PageLinks<T> Links { get; init; }

    /// <summary>
    ///     The previous page of results or null when there is no more results
    /// </summary>
    public Task<Page<T>> PreviousPage()
    {
        return Links.Prev?.Follow();
    }

    /// <summary>
    ///     The next page of results or null when there is no more results
    /// </summary>
    /// <returns></returns>
    public Task<Page<T>> NextPage()
    {
        return Links.Next?.Follow();
    }

    public class EmbeddedRecords
    {
        [JsonProperty(PropertyName = "records")]
        public List<T> Records { get; init; }
    }

    /// <summary>
    ///     Links connected to page response.
    /// </summary>
    public class PageLinks<T>
    {
        public Link<Page<T>> Next { get; init; }

        public Link<Page<T>> Prev { get; init; }

        public Link<Page<T>> Self { get; init; }
    }
}