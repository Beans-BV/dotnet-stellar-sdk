using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Responses
{
    /// <summary>
    ///     Represents page of objects.
    /// </summary>
    public class Page<T> : Response
    {
        [JsonPropertyName("_embedded")]
        public EmbeddedRecords Embedded { get; init; }

        public List<T> Records => Embedded.Records;

        [JsonPropertyName("_links")]
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
            [JsonPropertyName("records")]
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
}
