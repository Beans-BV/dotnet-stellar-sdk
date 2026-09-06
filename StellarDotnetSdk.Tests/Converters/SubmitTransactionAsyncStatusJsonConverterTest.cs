using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for the strict converter behind <see cref="SubmitTransactionAsyncResponse.TransactionStatus" />.
///     Focus: the wire values Horizon actually emits for <c>POST /transactions_async</c> (passed through verbatim
///     from stellar-core: "PENDING", "DUPLICATE", "TRY_AGAIN_LATER", "ERROR"), and the values the standard
///     <c>JsonStringEnumConverter</c> would have accepted in their place.
/// </summary>
[TestClass]
public class SubmitTransactionAsyncStatusJsonConverterTest
{
    /// <summary>
    ///     Verifies the four literals round-trip through the SDK's real shared options, not a hand-built bag —
    ///     which is the only thing that proves the converter is actually reachable.
    /// </summary>
    [DataTestMethod]
    [DataRow("PENDING", SubmitTransactionAsyncResponse.TransactionStatus.PENDING)]
    [DataRow("DUPLICATE", SubmitTransactionAsyncResponse.TransactionStatus.DUPLICATE)]
    [DataRow("TRY_AGAIN_LATER", SubmitTransactionAsyncResponse.TransactionStatus.TRY_AGAIN_LATER)]
    [DataRow("ERROR", SubmitTransactionAsyncResponse.TransactionStatus.ERROR)]
    public void RoundTrip_WithDefaultOptions_RoundTripsAllStatuses(string wireValue,
        SubmitTransactionAsyncResponse.TransactionStatus expected)
    {
        var deserialized = JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>(
            $"\"{wireValue}\"", JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(expected, JsonOptions.DefaultOptions);

        Assert.AreEqual(expected, deserialized);
        Assert.AreEqual($"\"{wireValue}\"", serialized);
    }

    /// <summary>
    ///     The regression guard for the strictness fix. Every non-boolean, non-null row below was accepted before a
    ///     strict converter was registered ahead of the catch-all <c>JsonStringEnumConverter</c>, which is
    ///     case-insensitive and maps bare integers by ordinal. The integer rows are the dangerous ones: ordinal 0 is
    ///     <see cref="SubmitTransactionAsyncResponse.TransactionStatus.PENDING" />, so <c>"tx_status": 0</c> read a
    ///     malformed response as the most optimistic status, and <c>"tx_status": 99</c> produced an undefined enum
    ///     value that silently matches none of the four members.
    /// </summary>
    [DataTestMethod]
    [DataRow("0")]
    [DataRow("1")]
    [DataRow("3")]
    [DataRow("99")]
    [DataRow("\"pending\"")]
    [DataRow("\"Pending\"")]
    [DataRow("\"try_again_later\"")]
    [DataRow("\"BOGUS\"")]
    [DataRow("null")]
    [DataRow("true")]
    public void Deserialize_WithDefaultOptions_RejectsWhatOnlyTheStandardConverterWouldAccept(string json)
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>(json,
                JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Pins the non-string-token guard specifically. Every row above also throws <see cref="JsonException" />
    ///     without that guard — System.Text.Json wraps the <see cref="System.InvalidOperationException" /> that
    ///     <c>reader.GetString()</c> raises on a non-string token — so asserting the exception type alone cannot
    ///     tell the guard from its absence, and deleting the guard left the whole suite green. Asserting the
    ///     message is what makes the guard load-bearing.
    /// </summary>
    [DataTestMethod]
    [DataRow("0", "Number")]
    [DataRow("true", "True")]
    [DataRow("[]", "StartArray")]
    [DataRow("{}", "StartObject")]
    public void Deserialize_WithNonStringToken_NamesTheTokenItFound(string json, string expectedTokenType)
    {
        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>(json,
                JsonOptions.DefaultOptions));

        StringAssert.Contains(exception.Message, "Expected a string value");
        StringAssert.Contains(exception.Message, expectedTokenType);
    }

    /// <summary>
    ///     Verifies the converter is reached through a whole <see cref="SubmitTransactionAsyncResponse" />, not just
    ///     when the enum is deserialized in isolation.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithinResponse_RejectsOrdinalStatus()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
                """{"tx_status":0,"hash":"aa"}""", JsonOptions.DefaultOptions));

        var ok = JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
            """{"tx_status":"PENDING","hash":"aa"}""", JsonOptions.DefaultOptions);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, ok!.TxStatus);
        Assert.AreEqual("aa", ok.Hash);
    }

    /// <summary>
    ///     Verifies the property-level <c>[JsonConverter]</c> pin: the strict wire format must hold whichever
    ///     options instance the response is deserialized with, not only under
    ///     <see cref="JsonOptions.DefaultOptions" />. Without the pin, a plain options bag maps bare integers by
    ///     ordinal (and rejects the string literals Horizon actually sends).
    /// </summary>
    [TestMethod]
    public void Deserialize_WithForeignOptions_StillEnforcesTheWireFormat()
    {
        var foreignOptions = new JsonSerializerOptions();

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
                """{"tx_status":0,"hash":"aa"}""", foreignOptions));

        var ok = JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
            """{"tx_status":"PENDING","hash":"aa"}""", foreignOptions);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, ok!.TxStatus);
    }

    /// <summary>
    ///     Verifies that an absent <c>tx_status</c> is rejected rather than defaulting to the zero member
    ///     (<c>PENDING</c>). An enum is a value type, so <c>RespectNullableAnnotations</c> cannot catch this — only
    ///     the <c>required</c> modifier on the property can. This documents behavior that already held before the
    ///     strict converter and must keep holding after it.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutTxStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
                """{"hash":"aa"}""", JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that an undefined value cannot be written, so the converter round-trips: without the check,
    ///     <c>Write</c> emits the bare number and <c>Read</c> then rejects it.
    /// </summary>
    [TestMethod]
    public void Serialize_WithUndefinedStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize((SubmitTransactionAsyncResponse.TransactionStatus)99,
                JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Guards the registration order this converter depends on: the catch-all must stay last, or it shadows
    ///     this converter and every rejection above silently becomes an acceptance.
    /// </summary>
    [TestMethod]
    public void DefaultOptions_RegistersTheStandardEnumConverterLast()
    {
        var converters = JsonOptions.DefaultOptions.Converters;
        var standardIndex = -1;
        var specificIndex = -1;
        for (var i = 0; i < converters.Count; i++)
        {
            switch (converters[i])
            {
                case System.Text.Json.Serialization.JsonStringEnumConverter:
                    standardIndex = i;
                    break;
                case SubmitTransactionAsyncStatusJsonConverter:
                    specificIndex = i;
                    break;
            }
        }

        Assert.AreNotEqual(-1, standardIndex, "The standard enum converter is no longer registered.");
        Assert.AreNotEqual(-1, specificIndex, "SubmitTransactionAsyncStatusJsonConverter is not registered.");
        Assert.IsTrue(specificIndex < standardIndex,
            "SubmitTransactionAsyncStatusJsonConverter must precede JsonStringEnumConverter, which matches every " +
            "enum and would otherwise shadow it.");
    }
}
