using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for <see cref="TransactionStatusJsonConverter" />.
///     Focus: the wire values Stellar RPC actually emits for <c>getTransaction</c>, and the values the standard
///     <c>JsonStringEnumConverter</c> would have accepted in their place.
/// </summary>
[TestClass]
public class TransactionStatusJsonConverterTest
{
    /// <summary>
    ///     Verifies the three literals round-trip through the SDK's real shared options, not a hand-built bag —
    ///     which is the only thing that proves the converter is actually reachable.
    /// </summary>
    [DataTestMethod]
    [DataRow("NOT_FOUND", TransactionInfo.TransactionStatus.NOT_FOUND)]
    [DataRow("SUCCESS", TransactionInfo.TransactionStatus.SUCCESS)]
    [DataRow("FAILED", TransactionInfo.TransactionStatus.FAILED)]
    public void RoundTrip_WithDefaultOptions_RoundTripsAllStatuses(string wireValue,
        TransactionInfo.TransactionStatus expected)
    {
        var deserialized = JsonSerializer.Deserialize<TransactionInfo.TransactionStatus>(
            $"\"{wireValue}\"", JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(expected, JsonOptions.DefaultOptions);

        Assert.AreEqual(expected, deserialized);
        Assert.AreEqual($"\"{wireValue}\"", serialized);
    }

    /// <summary>
    ///     The regression guard for the converter-ordering fix. Every row below was accepted before this converter
    ///     was registered ahead of the catch-all <c>JsonStringEnumConverter</c>, which is case-insensitive and maps
    ///     bare integers by ordinal. The integer rows are the dangerous ones: ordinal 1 is
    ///     <see cref="TransactionInfo.TransactionStatus.SUCCESS" />, so <c>"status": 1</c> presented an unsettled
    ///     transaction as successful on the endpoint callers poll to confirm payment, and <c>"status": 7</c>
    ///     produced an undefined enum value that silently matches none of the three members.
    /// </summary>
    [DataTestMethod]
    [DataRow("0", "Expected a string value")]
    [DataRow("1", "Expected a string value")]
    [DataRow("2", "Expected a string value")]
    [DataRow("7", "Expected a string value")]
    [DataRow("\"success\"", "cannot be converted")]
    [DataRow("\"Success\"", "cannot be converted")]
    [DataRow("\"not_found\"", "cannot be converted")]
    [DataRow("\"BOGUS\"", "cannot be converted")]
    [DataRow("null", "Expected a string value")]
    [DataRow("true", "Expected a string value")]
    public void Deserialize_WithDefaultOptions_RejectsWhatOnlyTheStandardConverterWouldAccept(string json,
        string expectedMessagePart)
    {
        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<TransactionInfo.TransactionStatus>(json, JsonOptions.DefaultOptions));

        // Pins the converter's own guard messages: System.Text.Json also wraps a reader failure in a
        // JsonException, so asserting the type alone cannot tell the hand-written guard from the fallback.
        StringAssert.Contains(exception.Message, expectedMessagePart);
    }

    /// <summary>
    ///     Verifies the converter is reached through a whole <see cref="TransactionInfo" />, not just when the enum
    ///     is deserialized in isolation.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithinTransactionInfo_RejectsOrdinalStatus()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<TransactionInfo>("{\"status\":1}", JsonOptions.DefaultOptions));

        var ok = JsonSerializer.Deserialize<TransactionInfo>(
            "{\"status\":\"SUCCESS\"}", JsonOptions.DefaultOptions);
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, ok!.Status);
    }

    /// <summary>
    ///     Verifies that an absent <c>status</c> is rejected rather than defaulting to the zero member. An enum is a
    ///     value type, so <c>RespectNullableAnnotations</c> cannot catch this — only <c>[JsonRequired]</c> can.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<TransactionInfo>("{\"ledger\":42}", JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies the property-level <c>[JsonConverter]</c> pin on <see cref="TransactionInfo.Status" />: a
    ///     consumer deserializing with their own options — even ones registering the permissive
    ///     <see cref="System.Text.Json.Serialization.JsonStringEnumConverter" /> — still gets strict parsing,
    ///     because System.Text.Json resolves a property attribute ahead of the options' converter collection.
    ///     Registration on <see cref="JsonOptions.DefaultOptions" /> alone would leave this path wide open.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithConsumerOwnedOptions_StillRejectsOrdinalStatus()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<TransactionInfo>("{\"status\":1}", options));

        var ok = JsonSerializer.Deserialize<TransactionInfo>("{\"status\":\"SUCCESS\"}", options);
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, ok!.Status);
    }

    /// <summary>
    ///     Verifies that an undefined value cannot be written, so the converter round-trips: without the check,
    ///     <c>Write</c> emitted the bare number as a string and <c>Read</c> then rejected it.
    /// </summary>
    [TestMethod]
    public void Serialize_WithUndefinedStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize((TransactionInfo.TransactionStatus)99, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Guards the registration order this converter depends on: the catch-all must stay last, or it shadows
    ///     this converter and every rejection above silently becomes an acceptance.
    /// </summary>
    [TestMethod]
    public void DefaultOptions_RegistersTheStandardEnumConverterLast()
    {
        var converters = JsonOptions.DefaultOptions.Converters;
        var specific = converters.ToList().FindIndex(c => c is TransactionStatusJsonConverter);
        var catchAll = converters.ToList().FindIndex(c => c is System.Text.Json.Serialization.JsonStringEnumConverter);

        Assert.IsTrue(specific >= 0, "TransactionStatusJsonConverter is not registered.");
        Assert.IsTrue(catchAll >= 0, "JsonStringEnumConverter is not registered.");
        Assert.IsTrue(specific < catchAll,
            $"TransactionStatusJsonConverter (index {specific}) must precede JsonStringEnumConverter " +
            $"(index {catchAll}), which matches every enum and would otherwise shadow it.");
    }
}
