using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace stellar_dotnet_sdk.responses;

/// <summary>
///     Represents page of objects.
///     https://www.stellar.org/developers/horizon/reference/resources/page.html
/// </summary>
/// <typeparam name="T"></typeparam>
public class Page<T> : Response
{
    public Page(EmbeddedRecords embedded, PageLinks<T> links)
    {
        Embedded = embedded;
        Links = links;
    }

    [JsonProperty(PropertyName = "_embedded")]
    public EmbeddedRecords Embedded { get; set; }

    public List<T> Records => Embedded.Records;

    [JsonProperty(PropertyName = "_links")]
    public PageLinks<T> Links { get; private set; }

    /// <summary>
    ///     The previous page of results or null when there is no more results
    /// </summary>
    /// <returns></returns>
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
        public List<T> Records { get; private set; }
    }
}