using System;
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

    /// <summary>
    ///     Serializes a single-filter request and returns the raw <c>filters[0].type</c> value, so assertions
    ///     compare against exactly what would go on the wire.
    /// </summary>
    private static string? SerializeFilterType(EventFilterType? type)
    {
        var request = new GetEventsRequest
        {
            StartLedger = 100,
            Filters =
            [
                new GetEventsRequest.EventFilter
                {
                    Type = type,
                    ContractIds = ["CDLZFC3SYJYDZT7K67VZ75HPJVIEUVNIXF47ZG2FB2RMQQVU2HHGCYSC"],
                },
            ],
        };

        using var document = Serialize(request);
        var element = document.RootElement.GetProperty("filters")[0].GetProperty("type");
        return element.ValueKind == JsonValueKind.Null ? null : element.GetString();
    }

    /// <summary>
    ///     Verifies that combined flags are joined with a bare comma. Stellar RPC splits the value on <c>","</c>
    ///     without trimming, so the <c>"System, Contract"</c> that a plain enum converter would emit is rejected
    ///     with <c>filter type invalid: if set, type must be either 'system' or 'contract'</c>. This is the
    ///     regression guard for the converter-resolution order: <c>JsonOptions.DefaultOptions</c> registers a
    ///     catch-all <c>JsonStringEnumConverter</c>, which outranks a type-level <c>[JsonConverter]</c>.
    /// </summary>
    [TestMethod]
    public void Serialize_WithCombinedEventTypes_ProducesCommaJoinedTypeWithoutSpaces()
    {
        Assert.AreEqual("system,contract", SerializeFilterType(EventFilterType.System | EventFilterType.Contract));
    }

    /// <summary>
    ///     Verifies that a single event type serializes to its bare lowercase literal.
    /// </summary>
    [TestMethod]
    public void Serialize_WithSingleEventType_ProducesLowercaseLiteral()
    {
        Assert.AreEqual("contract", SerializeFilterType(EventFilterType.Contract));
        Assert.AreEqual("system", SerializeFilterType(EventFilterType.System));
    }

    /// <summary>
    ///     Verifies that omitting the type leaves a JSON null, which Stellar RPC unmarshals into an empty type set
    ///     (i.e. no type filter) exactly as it did before the property became an enum.
    /// </summary>
    [TestMethod]
    public void Serialize_WithoutEventType_ProducesNullType()
    {
        Assert.IsNull(SerializeFilterType(null));
    }

    /// <summary>
    ///     Verifies that <see cref="EventFilterType.None" /> reaches the wire as the empty string, which RPC
    ///     accepts and treats as "no type filter".
    /// </summary>
    [TestMethod]
    public void Serialize_WithNone_ProducesEmptyString()
    {
        Assert.AreEqual("", SerializeFilterType(EventFilterType.None));
    }

    /// <summary>
    ///     Verifies that an undefined flag combination is rejected where it is assigned, so the mistake surfaces in
    ///     the caller's own stack frame rather than as a <c>-32602</c> from the server.
    /// </summary>
    [TestMethod]
    public void Type_WithUndefinedFlags_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            new GetEventsRequest.EventFilter { Type = (EventFilterType)99 });
    }

    /// <summary>
    ///     Verifies that every defined flag combination is accepted by the setter.
    /// </summary>
    [TestMethod]
    public void Type_WithDefinedFlags_IsAccepted()
    {
        Assert.AreEqual(EventFilterType.None, new GetEventsRequest.EventFilter { Type = EventFilterType.None }.Type);
        Assert.AreEqual(EventFilterType.System,
            new GetEventsRequest.EventFilter { Type = EventFilterType.System }.Type);
        Assert.AreEqual(EventFilterType.Contract,
            new GetEventsRequest.EventFilter { Type = EventFilterType.Contract }.Type);
        Assert.AreEqual(EventFilterType.System | EventFilterType.Contract,
            new GetEventsRequest.EventFilter
            {
                Type = EventFilterType.System | EventFilterType.Contract,
            }.Type);
        Assert.IsNull(new GetEventsRequest.EventFilter { Type = null }.Type);
    }

    /// <summary>
    ///     Verifies that a filter round-trips: the value the SDK writes is the value it reads back.
    /// </summary>
    [TestMethod]
    public void Deserialize_AfterSerialize_RoundTripsEventFilterType()
    {
        var filter = new GetEventsRequest.EventFilter
        {
            Type = EventFilterType.System | EventFilterType.Contract,
        };

        var json = JsonSerializer.Serialize(filter, JsonOptions.DefaultOptions);
        var restored = JsonSerializer.Deserialize<GetEventsRequest.EventFilter>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(restored);
        Assert.AreEqual(EventFilterType.System | EventFilterType.Contract, restored.Type);
    }
}
