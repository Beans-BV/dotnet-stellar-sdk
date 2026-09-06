using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing <see cref="SimulateTransactionResponse" /> from Stellar RPC payloads.
/// </summary>
[TestClass]
public class SimulateTransactionResponseDeserializerTest
{
    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> within the 32-bit range is deserialized unchanged.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSmallMinResourceFee_ReturnsValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"100","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(100L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> above <see cref="uint.MaxValue" /> deserializes at full width.
    ///     Stellar RPC declares the field as an <c>int64</c>, and values beyond 4 294 967 295 stroops (~429 XLM) are
    ///     reachable on large uploads and restores.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMinResourceFeeAboveUintMaxValue_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"5000000000","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(5_000_000_000L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that the largest value the <c>int64</c> wire type can carry deserializes without loss.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMaximumInt64MinResourceFee_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"9223372036854775807","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(long.MaxValue, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that an omitted <c>minResourceFee</c> — the shape Stellar RPC returns for a failed simulation —
    ///     stays null.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutMinResourceFee_ReturnsNull()
    {
        // Arrange
        const string json = """{"error":"host invocation failed","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that <see cref="SimulateTransactionResponse.SorobanAuthorization" /> reports "no entries" rather
    ///     than throwing when the response carries no <c>results</c> at all. The property indexes <c>Results[0]</c>,
    ///     so without the length half of its guard this dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the same for an empty <c>results</c> array, which is the shape Stellar RPC returns for a
    ///     simulation that produced no results. Without the length half of the guard this throws
    ///     <see cref="System.IndexOutOfRangeException" />.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithEmptyResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the element half of the guard, which is not redundant with the length check: <c>[null]</c> has
    ///     length one and would then be dereferenced. A conforming server cannot send it — the Go type is a slice of
    ///     structs, not pointers — but the property exists to behave predictably for responses that are not
    ///     conforming.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithNullResultElement_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[null],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies that a result carrying no <c>auth</c> key reports "no entries" rather than throwing. Stellar RPC
    ///     tags the field <c>auth,omitempty</c>, so this is the ordinary shape for a simulation that requires no
    ///     authorization; without the guard the entry loop dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutAuth_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[{"xdr":"AAAAAQ=="}],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies that a <c>stateChanges</c> entry with no <c>type</c> is rejected rather than leaving the
    ///     non-nullable <see cref="SimulateTransactionResponse.LedgerEntryChange.Type" /> holding
    ///     <see langword="null" />. <c>RespectNullableAnnotations</c> catches an explicit <c>null</c> but never an
    ///     absent property, so only <c>[JsonRequired]</c> closes this.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithStateChangeMissingType_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"key":"AAAA"}]}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        // System.Text.Json names the CLR property, not the JSON one. Assert on that exact phrase so the test
        // cannot pass on the incidental "type" inside "JSON deserialization for type '...'".
        StringAssert.Contains(exception.Message, "missing required properties including: 'Type'");
    }

    /// <summary>
    ///     Verifies that a <c>null</c> <em>element</em> in <c>stateChanges</c> is rejected at deserialization.
    ///     <c>RespectNullableAnnotations</c> constrains the array reference, never its contents, so this
    ///     previously produced an array holding <see langword="null" /> despite the non-nullable element type —
    ///     and the very loop <c>[JsonRequired]</c> on <c>Type</c> was added to protect then threw
    ///     <see cref="System.NullReferenceException" /> instead of <see cref="JsonException" />.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithNullStateChangeElement_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"type":"created"},null]}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "null element at index 1");

        // Positive control: the same payload without the null element deserializes, so the rejection is about
        // the null and not about the surrounding shape.
        var ok = JsonSerializer.Deserialize<SimulateTransactionResponse>(
            """{"latestLedger":1,"stateChanges":[{"type":"created"}]}""", JsonOptions.DefaultOptions);
        Assert.AreEqual("created", ok!.StateChanges![0].Type);
    }

    /// <summary>
    ///     Verifies that the empty <c>type</c> Stellar RPC v23.0.0/v23.0.1 emitted for pre-allocated no-op state
    ///     changes still deserializes: <c>[JsonRequired]</c> enforces presence, not membership of the three
    ///     documented literals.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEmptyStateChangeType_IsAccepted()
    {
        const string json = """{"latestLedger":1,"stateChanges":[{"type":""}]}""";

        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(response);
        Assert.AreEqual("", response.StateChanges![0].Type);
        Assert.IsNull(response.StateChanges[0].Key);
    }

    /// <summary>
    ///     Verifies that a <c>restorePreamble</c> with no <c>minResourceFee</c> is rejected. The property is a
    ///     value type, so without <c>[JsonRequired]</c> a truncated preamble silently yielded a zero restore fee
    ///     and a caller following the documented flow would submit the RestoreFootprint operation underfunded.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithRestorePreambleMissingMinResourceFee_ThrowsJsonException()
    {
        const string json = """{"latestLedger":1,"restorePreamble":{"transactionData":"AAAA"}}""";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions));
        StringAssert.Contains(exception.Message, "missing required properties including: 'MinResourceFee'");
    }

    /// <summary>
    ///     Verifies that serializing a response does not invoke the decoding
    ///     <see cref="SimulateTransactionResponse.SorobanAuthorization" /> getter. Serialization reads every
    ///     property, so without <c>[JsonIgnore]</c> an <see cref="InvalidDataException" /> escapes from inside
    ///     <see cref="JsonSerializer.Serialize{TValue}(TValue,JsonSerializerOptions)" /> for a caller who is only
    ///     trying to log or cache the response.
    /// </summary>
    [TestMethod]
    public void Serialize_WithMalformedAuthEntry_DoesNotInvokeTheDecodingGetter()
    {
        // "AAAAZAAAAAA=" is eight bytes whose credentials discriminant (100) no decoder recognises.
        const string json = """{"latestLedger":42,"results":[{"auth":["AAAAZAAAAAA="],"xdr":"AAAAAQ=="}]}""";
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(response);
        // Positive control: the getter really does throw for this payload, so the assertion below is not vacuous.
        Assert.ThrowsException<InvalidDataException>(() => _ = response.SorobanAuthorization);

        var serialized = JsonSerializer.Serialize(response, JsonOptions.DefaultOptions);

        StringAssert.Contains(serialized, "\"Results\"");
        Assert.IsFalse(serialized.Contains("SorobanAuthorization"),
            "SorobanAuthorization must not be part of the serialized payload; it is derived from Results[0].Auth.");
    }

    /// <summary>
    ///     Verifies that an auth entry naming a credentials discriminant no decoder recognises is normalized to
    ///     <see cref="InvalidDataException" /> with the originating exception preserved, rather than leaking the
    ///     generated decoder's own exception out of a property getter.
    /// </summary>
    /// <remarks>
    ///     This is the "unknown discriminant" arm specifically — eight bytes are enough to reach it, and it is a
    ///     different path from the truncation and invalid-base64 cases. Note that the
    ///     <see cref="InvalidOperationException" /> arm of the getter's filter (raised by
    ///     <c>SorobanCredentials.FromXdr</c>'s default case) is <em>not</em> reachable from here and so has no
    ///     test: the XDR enum defines exactly discriminants 0-3, <c>SorobanCredentials.FromXdr</c> handles all
    ///     four, and the generated <c>SorobanCredentialsType.Decode</c> rejects anything outside that range with
    ///     <see cref="InvalidDataException" /> before the dispatch is reached. That arm is a backstop against a
    ///     future protocol bump regenerating the enum, not a live path.
    /// </remarks>
    [TestMethod]
    public void SorobanAuthorization_WithUnknownCredentialsDiscriminant_ThrowsInvalidDataException()
    {
        // 00 00 00 64 00 00 00 00 — credentials discriminant 100, which no decoder recognises.
        const string json = """{"results":[{"auth":["AAAAZAAAAAA="]}]}""";
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(response);

        var exception = Assert.ThrowsException<InvalidDataException>(() => _ = response.SorobanAuthorization);
        StringAssert.Contains(exception.Message, "Malformed authorization entry XDR at index 0");
        // The originating decoder exception must survive as InnerException, not be flattened away.
        Assert.IsNotNull(exception.InnerException);
        StringAssert.Contains(exception.InnerException.Message, "100");
    }
}
