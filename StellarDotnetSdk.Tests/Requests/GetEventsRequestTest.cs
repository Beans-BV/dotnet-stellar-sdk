using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for the wire format <see cref="GetEventsRequest" /> produces, which is what Stellar RPC validates
///     the <c>getEvents</c> parameters against.
/// </summary>
[TestClass]
public class GetEventsRequestTest
{
    private const string Cursor = "0000000021474840576-0000000000";

    /// <summary>
    ///     Serializes a request the way <c>StellarRpcServer</c> does, so assertions compare against exactly what
    ///     would go on the wire.
    /// </summary>
    private static JsonDocument Serialize(GetEventsRequest request)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(request, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that pagination reaches the wire. <see cref="GetEventsRequest.PaginationOptions" /> declared
    ///     <c>Cursor</c> and <c>Limit</c> as public fields, which System.Text.Json ignores unless
    ///     <c>IncludeFields</c> is set — and <see cref="JsonOptions.DefaultOptions" /> does not set it. Both members
    ///     therefore serialized to an empty <c>"pagination": {}</c>, so the server silently applied its own
    ///     defaults: a request with <c>Limit = 2</c> came back with 100 events.
    /// </summary>
    [TestMethod]
    public void Serialize_WithPagination_IncludesCursorAndLimit()
    {
        var request = new GetEventsRequest
        {
            StartLedger = 100,
            Pagination = new GetEventsRequest.PaginationOptions { Cursor = Cursor, Limit = 2 },
        };

        using var document = Serialize(request);
        var pagination = document.RootElement.GetProperty("pagination");

        Assert.IsTrue(pagination.TryGetProperty("cursor", out var cursor), "pagination.cursor is missing.");
        Assert.AreEqual(Cursor, cursor.GetString());
        Assert.IsTrue(pagination.TryGetProperty("limit", out var limit), "pagination.limit is missing.");
        Assert.AreEqual(2L, limit.GetInt64());
    }

    /// <summary>
    ///     Verifies the shape an actual paging loop sends: a cursor with no <c>startLedger</c>, which Stellar RPC
    ///     requires to be omitted once a cursor is supplied. A dropped cursor made this request re-read the first
    ///     page forever.
    /// </summary>
    [TestMethod]
    public void Serialize_WithCursorOnlyPagination_IncludesCursor()
    {
        var request = new GetEventsRequest
        {
            Pagination = new GetEventsRequest.PaginationOptions { Cursor = Cursor },
        };

        using var document = Serialize(request);
        var pagination = document.RootElement.GetProperty("pagination");

        Assert.IsTrue(pagination.TryGetProperty("cursor", out var cursor), "pagination.cursor is missing.");
        Assert.AreEqual(Cursor, cursor.GetString());
    }

    /// <summary>
    ///     Verifies that pagination round-trips, guarding the read path as well: fields are invisible to the
    ///     deserializer for the same reason they are invisible to the serializer.
    /// </summary>
    [TestMethod]
    public void Deserialize_AfterSerialize_RoundTripsPagination()
    {
        var request = new GetEventsRequest
        {
            StartLedger = 100,
            Pagination = new GetEventsRequest.PaginationOptions { Cursor = Cursor, Limit = 2 },
        };

        var json = JsonSerializer.Serialize(request, JsonOptions.DefaultOptions);
        var restored = JsonSerializer.Deserialize<GetEventsRequest>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(restored);
        Assert.IsNotNull(restored.Pagination);
        Assert.AreEqual(Cursor, restored.Pagination.Cursor);
        Assert.AreEqual(2L, restored.Pagination.Limit);
    }
}
