using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Unit tests for <see cref="EventFilterTypeJsonConverter" />, which maps <see cref="EventFilterType" /> to and
///     from the single comma-separated string Stellar RPC uses for an event filter's <c>type</c> field.
/// </summary>
[TestClass]
public class EventFilterTypeJsonConverterTest
{
    /// <summary>
    ///     A bare <see cref="JsonSerializerOptions" /> — no converters registered. The type-level
    ///     <c>[JsonConverter]</c> on <see cref="EventFilterType" /> must carry the wire contract on its own, so that
    ///     it cannot be lost by serializing through options that happen not to register it.
    /// </summary>
    private static readonly JsonSerializerOptions BareOptions = new();

    /// <summary>
    ///     Verifies that each single event type serializes to its lowercase RPC literal.
    /// </summary>
    [TestMethod]
    public void Write_WithSingleEventType_WritesLowercaseWireValue()
    {
        Assert.AreEqual("\"system\"", JsonSerializer.Serialize(EventFilterType.System, BareOptions));
        Assert.AreEqual("\"contract\"", JsonSerializer.Serialize(EventFilterType.Contract, BareOptions));
    }

    /// <summary>
    ///     Verifies that combined flags are joined with a bare comma. Stellar RPC splits the value on <c>","</c>
    ///     without trimming, so the <c>", "</c> that System.Text.Json's built-in flags-enum converter would emit is
    ///     rejected server-side.
    /// </summary>
    [TestMethod]
    public void Write_WithCombinedEventTypes_JoinsWithCommaAndNoSpace()
    {
        Assert.AreEqual("\"system,contract\"",
            JsonSerializer.Serialize(EventFilterType.System | EventFilterType.Contract, BareOptions));
    }

    /// <summary>
    ///     Verifies that the converter also wins under <c>JsonOptions.DefaultOptions</c>, which registers a
    ///     catch-all <see cref="System.Text.Json.Serialization.JsonStringEnumConverter" /> for every enum. The
    ///     options' <c>Converters</c> collection outranks a type-level <c>[JsonConverter]</c> and the first entry
    ///     whose <c>CanConvert</c> matches is the one used, so this asserts the registration order in
    ///     <c>JsonOptions</c> as much as the converter itself.
    /// </summary>
    [TestMethod]
    public void Write_WithDefaultOptions_StillProducesTheRpcWireValue()
    {
        Assert.AreEqual("\"system,contract\"",
            JsonSerializer.Serialize(EventFilterType.System | EventFilterType.Contract, JsonOptions.DefaultOptions));
        Assert.AreEqual("\"contract\"",
            JsonSerializer.Serialize(EventFilterType.Contract, JsonOptions.DefaultOptions));
        Assert.AreEqual(EventFilterType.System | EventFilterType.Contract,
            JsonSerializer.Deserialize<EventFilterType>("\"system,contract\"", JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that <see cref="EventFilterType.None" /> serializes to the empty string, which Stellar RPC
    ///     accepts and treats as "no type filter".
    /// </summary>
    [TestMethod]
    public void Write_WithNone_WritesEmptyString()
    {
        Assert.AreEqual("\"\"", JsonSerializer.Serialize(EventFilterType.None, BareOptions));
    }

    /// <summary>
    ///     Verifies that a value carrying undefined flag bits is rejected client-side rather than being written to
    ///     the wire as a bare number for the server to refuse.
    /// </summary>
    [TestMethod]
    public void Write_WithUndefinedFlags_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            JsonSerializer.Serialize((EventFilterType)99, BareOptions));
    }

    /// <summary>
    ///     Verifies that the RPC literals round-trip back to the corresponding flags.
    /// </summary>
    [TestMethod]
    public void Read_WithValidWireValues_ReturnsFlags()
    {
        Assert.AreEqual(EventFilterType.System,
            JsonSerializer.Deserialize<EventFilterType>("\"system\"", BareOptions));
        Assert.AreEqual(EventFilterType.Contract,
            JsonSerializer.Deserialize<EventFilterType>("\"contract\"", BareOptions));
        Assert.AreEqual(EventFilterType.System | EventFilterType.Contract,
            JsonSerializer.Deserialize<EventFilterType>("\"system,contract\"", BareOptions));
        Assert.AreEqual(EventFilterType.None, JsonSerializer.Deserialize<EventFilterType>("\"\"", BareOptions));
    }

    /// <summary>
    ///     Verifies that values Stellar RPC refuses are refused here too, in the caller's own stack frame instead of
    ///     as a server round-trip error. <c>diagnostic</c> was legal up to RPC v22.1.5 but has been rejected since
    ///     v23.0.0, and RPC does not trim whitespace around the comma separator.
    /// </summary>
    [DataTestMethod]
    [DataRow("\"diagnostic\"")]
    [DataRow("\"system, contract\"")]
    [DataRow("\"System\"")]
    [DataRow("\"system,\"")]
    [DataRow("\"bogus\"")]
    [DataRow("123")]
    [DataRow("[\"system\"]")]
    public void Read_WithValueRpcRejects_ThrowsJsonException(string json)
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<EventFilterType>(json, BareOptions));
    }

    /// <summary>
    ///     Verifies that the type-level converter is applied to <see cref="Nullable{T}" /> as well, so that the
    ///     nullable <c>EventFilter.Type</c> property is covered by the same contract.
    /// </summary>
    [TestMethod]
    public void ReadAndWrite_WithNullableEventFilterType_UsesTheSameConverter()
    {
        Assert.AreEqual("\"contract\"",
            JsonSerializer.Serialize((EventFilterType?)EventFilterType.Contract, BareOptions));
        Assert.AreEqual("null", JsonSerializer.Serialize((EventFilterType?)null, BareOptions));
        Assert.AreEqual(EventFilterType.Contract,
            JsonSerializer.Deserialize<EventFilterType?>("\"contract\"", BareOptions));
        Assert.IsNull(JsonSerializer.Deserialize<EventFilterType?>("null", BareOptions));
    }
}
