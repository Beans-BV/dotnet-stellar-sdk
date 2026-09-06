using System.Collections.Generic;
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
        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize((TransactionInfo.TransactionStatus)99, JsonOptions.DefaultOptions));

        // Pin the message, not just the type: without this the guard could throw an empty JsonException and
        // every assertion here would still pass, leaving an operator with no way to tell which value was bad.
        Assert.AreEqual("Value '99' is not a defined TransactionStatus.", exception.Message);
    }

    /// <summary>
    ///     Pins the fail-closed trade-off the CHANGELOG documents: a <c>getTransactions</c> page is deserialized as
    ///     one document, so a single entry carrying an ordinal status fails the whole response rather than being
    ///     read as <see cref="TransactionInfo.TransactionStatus.SUCCESS" />.
    /// </summary>
    [TestMethod]
    public void Deserialize_GetTransactionsPageWithOneOrdinalStatus_FailsWholePage()
    {
        const string valid = "{\"status\":\"SUCCESS\",\"txHash\":\"aa\"}";
        const string ordinal = "{\"status\":1,\"txHash\":\"bb\"}";

        var ok = JsonSerializer.Deserialize<GetTransactionsResponse>(
            $"{{\"transactions\":[{valid},{valid}]}}", JsonOptions.DefaultOptions);
        Assert.AreEqual(2, ok!.Transactions!.Length);

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<GetTransactionsResponse>(
                $"{{\"transactions\":[{valid},{ordinal}]}}", JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies the pins reach <see cref="GetTransactionResponse" />, the subclass
    ///     <c>StellarRpcServer.GetTransaction</c> actually returns: it inherits both the property-level converter
    ///     (strict even under consumer-owned options) and <c>[JsonRequired]</c>.
    /// </summary>
    [TestMethod]
    public void Deserialize_GetTransactionResponse_InheritsStrictRequiredStatus()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<GetTransactionResponse>("{\"status\":1}", options));
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<GetTransactionResponse>("{\"latestLedger\":42}", options));

        var ok = JsonSerializer.Deserialize<GetTransactionResponse>("{\"status\":\"SUCCESS\"}", options);
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS, ok!.Status);
    }

    /// <summary>
    ///     Verifies the enum still works as a dictionary key under <see cref="JsonOptions.DefaultOptions" />. The
    ///     catch-all <c>JsonStringEnumConverter</c> this converter displaces supported keys; without the
    ///     property-name overloads the same call threw <see cref="System.NotSupportedException" />.
    /// </summary>
    [TestMethod]
    public void RoundTrip_AsDictionaryKey_WithDefaultOptions()
    {
        var original = new Dictionary<TransactionInfo.TransactionStatus, int>
        {
            [TransactionInfo.TransactionStatus.NOT_FOUND] = 0,
            [TransactionInfo.TransactionStatus.SUCCESS] = 1,
            [TransactionInfo.TransactionStatus.FAILED] = 2,
        };

        var json = JsonSerializer.Serialize(original, JsonOptions.DefaultOptions);
        var roundTripped = JsonSerializer.Deserialize<Dictionary<TransactionInfo.TransactionStatus, int>>(
            json, JsonOptions.DefaultOptions);

        Assert.AreEqual("{\"NOT_FOUND\":0,\"SUCCESS\":1,\"FAILED\":2}", json);
        CollectionAssert.AreEquivalent(original, roundTripped);
    }

    /// <summary>
    ///     Verifies dictionary keys are held to the same literal set as values: a key the value path would reject
    ///     is rejected, and an undefined value cannot be written as a key.
    /// </summary>
    [DataTestMethod]
    [DataRow("{\"success\":1}")]
    [DataRow("{\"1\":1}")]
    [DataRow("{\"BOGUS\":1}")]
    public void Deserialize_AsDictionaryKey_RejectsNonLiteralKeys(string json)
    {
        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<Dictionary<TransactionInfo.TransactionStatus, int>>(
                json, JsonOptions.DefaultOptions));

        StringAssert.Contains(exception.Message, "cannot be converted");
    }

    /// <summary>
    ///     Verifies an undefined value cannot be written as a dictionary key either.
    /// </summary>
    [TestMethod]
    public void Serialize_WithUndefinedStatusAsDictionaryKey_ThrowsJsonException()
    {
        var dictionary = new Dictionary<TransactionInfo.TransactionStatus, int>
        {
            [(TransactionInfo.TransactionStatus)99] = 1,
        };

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize(dictionary, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Guards the registration order this converter depends on: the catch-all must stay last, or it shadows
    ///     this converter and every rejection above silently becomes an acceptance.
    /// </summary>
    [TestMethod]
    public void DefaultOptions_RegistersTheStandardEnumConverterLast()
    {
        var converters = JsonOptions.DefaultOptions.Converters.ToList();
        var specific = converters.FindIndex(c => c is TransactionStatusJsonConverter);
        var catchAll = converters.FindIndex(c => c is System.Text.Json.Serialization.JsonStringEnumConverter);

        Assert.IsTrue(specific >= 0, "TransactionStatusJsonConverter is not registered.");
        Assert.IsTrue(catchAll >= 0, "JsonStringEnumConverter is not registered.");
        Assert.IsTrue(specific < catchAll,
            $"TransactionStatusJsonConverter (index {specific}) must precede JsonStringEnumConverter " +
            $"(index {catchAll}), which matches every enum and would otherwise shadow it.");
    }
}
