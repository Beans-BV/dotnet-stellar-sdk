using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents a paginated page of objects from the Horizon API.
///     Used to navigate through large result sets using cursor-based pagination.
/// </summary>
/// <typeparam name="T">The type of records contained in this page.</typeparam>
public sealed class Page<T> : Response
{
    /// <summary>
    ///     The embedded records container for HAL JSON format.
    /// </summary>
    [JsonPropertyName("_embedded")]
    public required EmbeddedRecords Embedded { get; init; }

    /// <summary>
    ///     The list of records in this page.
    /// </summary>
    public List<T> Records => Embedded.Records;

    /// <summary>
    ///     Navigation links for this page.
    /// </summary>
    [JsonPropertyName("_links")]
    public PageLinks<T>? Links { get; init; }

    /// <summary>
    ///     The previous page of results or null when there is no more results
    /// </summary>
    public Task<Page<T>>? PreviousPage()
    {
        return Links?.Prev.Follow();
    }

    /// <summary>
    ///     The next page of results or null when there is no more results
    /// </summary>
    public Task<Page<T>>? NextPage()
    {
        return Links?.Next.Follow();
    }

    /// <summary>
    ///     Container for embedded records in HAL JSON format.
    /// </summary>
    public sealed class EmbeddedRecords
    {
        /// <summary>
        ///     The list of records in this page.
        /// </summary>
        [JsonPropertyName("records")]
        public required List<T> Records { get; init; }
    }

    /// <summary>
    ///     Navigation links for paginated responses.
    /// </summary>
    public sealed class PageLinks<T>
    {
        /// <summary>
        ///     Link to the next page of results.
        /// </summary>
        public required Link<Page<T>> Next { get; init; }

        /// <summary>
        ///     Link to the previous page of results.
        /// </summary>
        public required Link<Page<T>> Prev { get; init; }

        /// <summary>
        ///     Link to this page.
        /// </summary>
        public required Link<Page<T>> Self { get; init; }
    }
}